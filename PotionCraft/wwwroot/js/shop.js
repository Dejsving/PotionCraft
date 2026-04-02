// ─── Магазин: клиентская логика ───────────────────────────────────────────
// rarityColors определён в _Layout.cshtml (глобальная область видимости)

// ─── Состояние ────────────────────────────────────────────────────────────
let shopInventory = [];          // ShopItem[] — ассортимент торговца
let playerBag = null;            // CharacterBag — сумка персонажа
let tradeBuy = new Map();        // Map<id, { item, quantity }>
let tradeSell = new Map();       // Map<id, { item, quantity, sellPrice }>
let activeShopFilter = 'all';
let activePlayerFilter = 'all';

// ─── Утилиты ──────────────────────────────────────────────────────────────

function getActiveCharacter() {
    try {
        const data = window.sessionStorage.getItem('active_character');
        return data ? JSON.parse(data) : null;
    } catch { return null; }
}

function getCharacterId() {
    const c = getActiveCharacter();
    return c ? (c.id || c.Id) : null;
}

function formatGold(value) {
    return value.toFixed(2) + ' зм';
}

function copperToDisplay(copper) {
    const g = Math.floor(copper / 100);
    const s = Math.floor((copper % 100) / 10);
    const c = copper % 10;
    return { gold: g, silver: s, copper: c };
}

function getRarityBadge(rarity, rarityName) {
    const color = rarityColors[rarity] || 'secondary';
    return `<span class="badge bg-${color}">${rarityName || 'Неизвестно'}</span>`;
}

// ─── Загрузка данных ───────────────────────────────────────────────────────

async function loadShopInventory() {
    const container = document.getElementById('shopInventoryList');
    container.innerHTML = `<div class="text-center text-muted py-4">
        <div class="spinner-border spinner-border-sm" role="status"></div>
        <span class="ms-2">Загрузка ассортимента...</span>
    </div>`;

    try {
        const res = await fetch('/api/shop/inventory');
        if (!res.ok) throw new Error('Ошибка загрузки');
        shopInventory = await res.json();
        renderShopInventory();
    } catch {
        container.innerHTML = '<p class="text-danger text-center py-4">Не удалось загрузить ассортимент</p>';
    }
}

async function loadPlayerInventory() {
    const container = document.getElementById('playerInventoryList');
    const balanceDiv = document.getElementById('playerBalance');
    const characterId = getCharacterId();

    if (!characterId) {
        container.innerHTML = '<p class="text-muted text-center py-4">Выберите персонажа</p>';
        balanceDiv.classList.add('d-none');
        return;
    }

    container.innerHTML = `<div class="text-center text-muted py-4">
        <div class="spinner-border spinner-border-sm" role="status"></div>
        <span class="ms-2">Загрузка инвентаря...</span>
    </div>`;

    try {
        const res = await fetch(`/api/characters/${characterId}/bag`);
        if (!res.ok) throw new Error('Ошибка загрузки');
        playerBag = await res.json();
        updatePlayerBalance();
        renderPlayerInventory();
    } catch {
        container.innerHTML = '<p class="text-danger text-center py-4">Не удалось загрузить инвентарь</p>';
    }
}

function updatePlayerBalance() {
    const balanceDiv = document.getElementById('playerBalance');
    if (!playerBag) {
        balanceDiv.classList.add('d-none');
        return;
    }

    const gold = playerBag.goldCoins ?? playerBag.GoldCoins ?? 0;
    const silver = playerBag.silverCoins ?? playerBag.SilverCoins ?? 0;
    const copper = playerBag.copperCoins ?? playerBag.CopperCoins ?? 0;

    document.getElementById('playerGold').textContent = gold;
    document.getElementById('playerSilver').textContent = silver;
    document.getElementById('playerCopper').textContent = copper;
    balanceDiv.classList.remove('d-none');
}

// ─── Рендер: ассортимент торговца ──────────────────────────────────────────

function renderShopInventory() {
    const container = document.getElementById('shopInventoryList');
    const filtered = shopInventory.filter(item => {
        if (item.availableQuantity <= 0) return false;
        if (activeShopFilter === 'all') return true;
        return item.category === activeShopFilter;
    });

    if (filtered.length === 0) {
        container.innerHTML = '<p class="text-muted text-center py-4">Нет товаров</p>';
        return;
    }

    // Группируем по редкости
    const grouped = {};
    for (const item of filtered) {
        if (!grouped[item.rarity]) grouped[item.rarity] = [];
        grouped[item.rarity].push(item);
    }

    let html = '';
    for (const rarity of Object.keys(grouped).sort((a, b) => a - b)) {
        const items = grouped[rarity];
        const first = items[0];
        html += `<div class="mb-2">
            ${getRarityBadge(first.rarity, first.rarityName)}
            <div class="list-group list-group-flush mt-1">`;

        for (const item of items) {
            // Учитываем товар, уже добавленный в сделку
            const inTrade = tradeBuy.has(item.id) ? tradeBuy.get(item.id).quantity : 0;
            const remaining = item.availableQuantity - inTrade;

            html += `<a href="#" class="list-group-item list-group-item-action py-1 px-2 d-flex justify-content-between align-items-center shop-item ${remaining <= 0 ? 'disabled text-muted' : ''}"
                data-id="${item.id}" data-category="${item.category}">
                <div>
                    <i class="bi bi-flower1 me-1"></i>
                    <span class="small">${item.name}</span>
                </div>
                <div class="text-end">
                    <span class="badge bg-dark rounded-pill me-1" title="Доступно">${remaining}</span>
                    <span class="small text-danger" title="Цена покупки">${formatGold(item.buyPrice)}</span>
                </div>
            </a>`;
        }

        html += '</div></div>';
    }

    container.innerHTML = html;
    bindShopItemClicks();
}

// ─── Рендер: инвентарь персонажа ───────────────────────────────────────────

function renderPlayerInventory() {
    const container = document.getElementById('playerInventoryList');

    if (!playerBag) {
        container.innerHTML = '<p class="text-muted text-center py-4">Выберите персонажа</p>';
        return;
    }

    let allItems = [];

    // Травы
    const herbs = playerBag.herbs || playerBag.Herbs || {};
    for (const key in herbs) {
        const entry = herbs[key];
        const herb = entry.herb || entry.Herb;
        const qty = entry.quantity ?? entry.Quantity ?? 0;
        if (!herb || qty <= 0) continue;

        // Учитываем товар, уже добавленный в сделку на продажу
        const inTrade = tradeSell.has(key) ? tradeSell.get(key).quantity : 0;
        const remaining = qty - inTrade;

        // Ищем цену продажи из ассортимента магазина
        const shopItem = shopInventory.find(si => si.id === key || si.id === (herb.id || herb.Id));
        const sellPrice = shopItem ? shopItem.sellPrice : 0;

        allItems.push({
            id: key,
            name: herb.name || herb.Name || 'Неизвестно',
            category: 'herb',
            rarity: herb.rarity ?? herb.Rarity ?? 0,
            rarityName: shopItem ? shopItem.rarityName : '',
            quantity: remaining,
            totalQuantity: qty,
            sellPrice: sellPrice
        });
    }

    // Зелья
    const potions = playerBag.potions || playerBag.Potions || {};
    for (const key in potions) {
        const entry = potions[key];
        const potion = entry.potion || entry.Potion;
        const qty = entry.quantity ?? entry.Quantity ?? 0;
        if (!potion || qty <= 0) continue;

        const inTrade = tradeSell.has(key) ? tradeSell.get(key).quantity : 0;
        const remaining = qty - inTrade;

        allItems.push({
            id: key,
            name: potion.name || potion.Name || 'Неизвестно',
            category: 'potion',
            rarity: 0,
            rarityName: '',
            quantity: remaining,
            totalQuantity: qty,
            sellPrice: 0
        });
    }

    // Яды
    const poisons = playerBag.poisons || playerBag.Poisons || {};
    for (const key in poisons) {
        const entry = poisons[key];
        const potion = entry.potion || entry.Potion;
        const qty = entry.quantity ?? entry.Quantity ?? 0;
        if (!potion || qty <= 0) continue;

        const inTrade = tradeSell.has(key) ? tradeSell.get(key).quantity : 0;
        const remaining = qty - inTrade;

        allItems.push({
            id: key,
            name: potion.name || potion.Name || 'Неизвестно',
            category: 'poison',
            rarity: 0,
            rarityName: '',
            quantity: remaining,
            totalQuantity: qty,
            sellPrice: 0
        });
    }

    // Фильтр
    const filtered = allItems.filter(item => {
        if (activePlayerFilter === 'all') return true;
        return item.category === activePlayerFilter;
    });

    if (filtered.length === 0) {
        container.innerHTML = '<p class="text-muted text-center py-4">Инвентарь пуст</p>';
        return;
    }

    let html = '';
    for (const item of filtered) {
        const icon = item.category === 'herb' ? 'bi-flower1' :
                     item.category === 'potion' ? 'bi-droplet-fill' : 'bi-exclamation-triangle-fill';

        html += `<a href="#" class="list-group-item list-group-item-action py-1 px-2 d-flex justify-content-between align-items-center player-item ${item.quantity <= 0 ? 'disabled text-muted' : ''}"
            data-id="${item.id}" data-category="${item.category}" data-sell-price="${item.sellPrice}">
            <div>
                <i class="bi ${icon} me-1"></i>
                <span class="small">${item.name}</span>
            </div>
            <div class="text-end">
                <span class="badge bg-dark rounded-pill me-1" title="В наличии">${item.quantity}</span>
                ${item.sellPrice > 0 ? `<span class="small text-success" title="Цена продажи">${formatGold(item.sellPrice)}</span>` : ''}
            </div>
        </a>`;
    }

    container.innerHTML = `<div class="list-group list-group-flush">${html}</div>`;
    bindPlayerItemClicks();
}

// ─── Рендер: текущая сделка ────────────────────────────────────────────────

function renderTradeBuy() {
    const container = document.getElementById('tradeBuyList');

    if (tradeBuy.size === 0) {
        container.innerHTML = '<p class="text-muted small text-center mb-0">Нажмите на товар слева, чтобы добавить</p>';
        return;
    }

    let html = '';
    for (const [id, entry] of tradeBuy) {
        html += `<div class="d-flex justify-content-between align-items-center mb-1 trade-buy-item" data-id="${id}">
            <div class="small text-truncate me-1" style="max-width: 45%;">
                <i class="bi bi-flower1 me-1"></i>${entry.item.name}
            </div>
            <div class="d-flex align-items-center">
                <span class="small text-danger me-2">${formatGold(entry.item.buyPrice * entry.quantity)}</span>
                <input type="number" class="form-control form-control-sm text-center trade-buy-qty" data-id="${id}" value="${entry.quantity}" min="1" max="${entry.item.availableQuantity}" style="width: 50px; padding: 0 2px; height: 24px;" />
                <button class="btn btn-outline-danger btn-sm py-0 px-1 ms-1 trade-buy-remove" data-id="${id}" title="Убрать">
                    <i class="bi bi-x"></i>
                </button>
            </div>
        </div>`;
    }

    container.innerHTML = html;
    bindTradeBuyControls();
}

function getMaxSellQty(id, category) {
    if (!playerBag) return 0;
    if (category === 'herb') {
        const herbs = playerBag.herbs || playerBag.Herbs || {};
        const bagEntry = herbs[id];
        if (bagEntry) return bagEntry.quantity ?? bagEntry.Quantity ?? 0;
    }
    return 0;
}

function renderTradeSell() {
    const container = document.getElementById('tradeSellList');

    if (tradeSell.size === 0) {
        container.innerHTML = '<p class="text-muted small text-center mb-0">Нажмите на предмет справа, чтобы добавить</p>';
        return;
    }

    let html = '';
    for (const [id, entry] of tradeSell) {
        const icon = entry.category === 'herb' ? 'bi-flower1' :
                     entry.category === 'potion' ? 'bi-droplet-fill' : 'bi-exclamation-triangle-fill';

        html += `<div class="d-flex justify-content-between align-items-center mb-1 trade-sell-item" data-id="${id}">
            <div class="small text-truncate me-1" style="max-width: 45%;">
                <i class="bi ${icon} me-1"></i>${entry.name}
            </div>
            <div class="d-flex align-items-center">
                <span class="small text-success me-2">${formatGold(entry.sellPrice * entry.quantity)}</span>
                <input type="number" class="form-control form-control-sm text-center trade-sell-qty" data-id="${id}" value="${entry.quantity}" min="1" max="${getMaxSellQty(id, entry.category)}" style="width: 50px; padding: 0 2px; height: 24px;" />
                <button class="btn btn-outline-danger btn-sm py-0 px-1 ms-1 trade-sell-remove" data-id="${id}" title="Убрать">
                    <i class="bi bi-x"></i>
                </button>
            </div>
        </div>`;
    }

    container.innerHTML = html;
    bindTradeSellControls();
}

function updateTradeBalance() {
    let totalBuy = 0;
    for (const [, entry] of tradeBuy) {
        totalBuy += entry.item.buyPrice * entry.quantity;
    }

    let totalSell = 0;
    for (const [, entry] of tradeSell) {
        totalSell += entry.sellPrice * entry.quantity;
    }

    const balance = totalSell - totalBuy;

    document.getElementById('totalBuyCost').textContent = formatGold(totalBuy);
    document.getElementById('totalSellRevenue').textContent = formatGold(totalSell);

    const balanceEl = document.getElementById('tradeBalance');
    if (balance >= 0) {
        balanceEl.textContent = `Ваш доход: ${formatGold(balance)}`;
        balanceEl.className = 'fw-bold text-success';
    } else {
        balanceEl.textContent = `К оплате: ${formatGold(Math.abs(balance))}`;
        balanceEl.className = 'fw-bold text-danger';
    }

    // Проверяем, может ли персонаж оплатить
    const btn = document.getElementById('btnApproveTrade');
    const errorDiv = document.getElementById('tradeError');

    const hasItems = tradeBuy.size > 0 || tradeSell.size > 0;
    const characterId = getCharacterId();
    const playerCoins = playerBag ? (playerBag.coins ?? playerBag.Coins ?? 0) : 0;
    const costCopper = Math.round(Math.abs(balance) * 100);

    if (!characterId) {
        btn.disabled = true;
        errorDiv.textContent = 'Выберите персонажа';
        errorDiv.classList.remove('d-none');
    } else if (!hasItems) {
        btn.disabled = true;
        errorDiv.classList.add('d-none');
    } else if (balance < 0 && costCopper > playerCoins) {
        btn.disabled = true;
        errorDiv.textContent = 'Недостаточно средств';
        errorDiv.classList.remove('d-none');
    } else {
        btn.disabled = false;
        errorDiv.classList.add('d-none');
    }
}

// ─── Обработчики кликов ────────────────────────────────────────────────────

function bindShopItemClicks() {
    document.querySelectorAll('.shop-item').forEach(el => {
        el.addEventListener('click', function (e) {
            e.preventDefault();
            if (this.classList.contains('disabled')) return;

            const id = this.dataset.id;
            const item = shopInventory.find(i => i.id === id);
            if (!item) return;

            const inTrade = tradeBuy.has(id) ? tradeBuy.get(id).quantity : 0;
            if (inTrade >= item.availableQuantity) return;

            if (tradeBuy.has(id)) {
                tradeBuy.get(id).quantity++;
            } else {
                tradeBuy.set(id, { item: item, quantity: 1 });
            }

            renderShopInventory();
            renderTradeBuy();
            updateTradeBalance();
        });
    });
}

function bindPlayerItemClicks() {
    document.querySelectorAll('.player-item').forEach(el => {
        el.addEventListener('click', function (e) {
            e.preventDefault();
            if (this.classList.contains('disabled')) return;

            const id = this.dataset.id;
            const category = this.dataset.category;
            const sellPrice = parseFloat(this.dataset.sellPrice) || 0;
            const nameEl = this.querySelector('.small');
            const name = nameEl ? nameEl.textContent : 'Неизвестно';

            // Определяем максимальное количество
            let maxQty = 0;
            if (category === 'herb') {
                const herbs = playerBag.herbs || playerBag.Herbs || {};
                const entry = herbs[id];
                if (entry) maxQty = entry.quantity ?? entry.Quantity ?? 0;
            }

            const inTrade = tradeSell.has(id) ? tradeSell.get(id).quantity : 0;
            if (inTrade >= maxQty) return;

            if (tradeSell.has(id)) {
                tradeSell.get(id).quantity++;
            } else {
                tradeSell.set(id, { name: name, category: category, sellPrice: sellPrice, quantity: 1 });
            }

            renderPlayerInventory();
            renderTradeSell();
            updateTradeBalance();
        });
    });
}

function bindTradeBuyControls() {
    document.querySelectorAll('.trade-buy-remove').forEach(btn => {
        btn.addEventListener('click', function () {
            const id = this.dataset.id;
            tradeBuy.delete(id);
            renderShopInventory();
            renderTradeBuy();
            updateTradeBalance();
        });
    });

    document.querySelectorAll('.trade-buy-qty').forEach(input => {
        input.addEventListener('change', function () {
            const id = this.dataset.id;
            if (!tradeBuy.has(id)) return;
            const entry = tradeBuy.get(id);
            let val = parseInt(this.value) || 0;
            if (val <= 0) {
                tradeBuy.delete(id);
            } else {
                entry.quantity = Math.min(val, entry.item.availableQuantity);
            }
            renderShopInventory();
            renderTradeBuy();
            updateTradeBalance();
        });
    });
}

function bindTradeSellControls() {
    document.querySelectorAll('.trade-sell-remove').forEach(btn => {
        btn.addEventListener('click', function () {
            const id = this.dataset.id;
            tradeSell.delete(id);
            renderPlayerInventory();
            renderTradeSell();
            updateTradeBalance();
        });
    });

    document.querySelectorAll('.trade-sell-qty').forEach(input => {
        input.addEventListener('change', function () {
            const id = this.dataset.id;
            if (!tradeSell.has(id)) return;
            const entry = tradeSell.get(id);
            let val = parseInt(this.value) || 0;
            const maxQty = getMaxSellQty(id, entry.category);
            if (val <= 0) {
                tradeSell.delete(id);
            } else {
                entry.quantity = Math.min(val, maxQty);
            }
            renderPlayerInventory();
            renderTradeSell();
            updateTradeBalance();
        });
    });
}

// ─── Фильтры ───────────────────────────────────────────────────────────────

function setupFilters() {
    document.querySelectorAll('#shopFilters .btn').forEach(btn => {
        btn.addEventListener('click', function () {
            document.querySelectorAll('#shopFilters .btn').forEach(b => b.classList.remove('active'));
            this.classList.add('active');
            activeShopFilter = this.dataset.filter;
            renderShopInventory();
        });
    });

    document.querySelectorAll('#playerFilters .btn').forEach(btn => {
        btn.addEventListener('click', function () {
            document.querySelectorAll('#playerFilters .btn').forEach(b => b.classList.remove('active'));
            this.classList.add('active');
            activePlayerFilter = this.dataset.filter;
            renderPlayerInventory();
        });
    });
}

// ─── Сделка ────────────────────────────────────────────────────────────────

async function executeTrade() {
    const characterId = getCharacterId();
    if (!characterId) return;

    const btn = document.getElementById('btnApproveTrade');
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status"></span>Обработка...';

    const itemsToBuy = [];
    for (const [id, entry] of tradeBuy) {
        itemsToBuy.push({ itemId: id, quantity: entry.quantity, category: entry.item.category });
    }

    const itemsToSell = [];
    for (const [id, entry] of tradeSell) {
        itemsToSell.push({ itemId: id, quantity: entry.quantity, category: entry.category });
    }

    try {
        const res = await fetch('/api/shop/trade', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                characterId: characterId,
                itemsToBuy: itemsToBuy,
                itemsToSell: itemsToSell
            })
        });

        const result = await res.json();

        if (res.ok && result.success) {
            // Формируем сообщение об изменении баланса
            const change = result.balanceChange;
            const display = copperToDisplay(Math.abs(change));
            let msg = 'Сделка успешна. ';
            if (change > 0) {
                msg += `Получено: ${display.gold} зм ${display.silver} см ${display.copper} мм`;
            } else if (change < 0) {
                msg += `Потрачено: ${display.gold} зм ${display.silver} см ${display.copper} мм`;
            } else {
                msg += 'Баланс не изменился.';
            }

            // Очищаем сделку
            tradeBuy.clear();
            tradeSell.clear();
            renderTradeBuy();
            renderTradeSell();

            // Перезагружаем данные из БД
            await loadShopInventory();
            await loadPlayerInventory();
            updateTradeBalance();

            // Toast-уведомление
            document.getElementById('tradeToastMessage').textContent = msg;
            const toast = new bootstrap.Toast(document.getElementById('tradeToast'));
            toast.show();
        } else {
            const errorDiv = document.getElementById('tradeError');
            errorDiv.textContent = result.message || 'Ошибка выполнения сделки';
            errorDiv.classList.remove('d-none');
        }
    } catch {
        const errorDiv = document.getElementById('tradeError');
        errorDiv.textContent = 'Ошибка соединения с сервером';
        errorDiv.classList.remove('d-none');
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<i class="bi bi-check-circle me-1"></i>Одобрить сделку';
        updateTradeBalance();
    }
}

// ─── Инициализация ─────────────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', function () {
    setupFilters();
    loadShopInventory();
    loadPlayerInventory();

    document.getElementById('btnApproveTrade').addEventListener('click', executeTrade);
});
