# Plan: Переписать PotionCraft.Client на React/TypeScript (Vite + Tailwind + Zustand + React Query)

## Решения (из уточняющих вопросов)
- Стек: Vite + React + TypeScript, SPA (без SSR).
- Стили: Tailwind CSS (полный отказ от Bootstrap в новом фронтенде).
- Состояние: Zustand (сессия/персонаж/сумка) + React Query (серверные данные, кэш, инвалидция).
- Хостинг: отдельный процесс/сервис для фронтенда (не через PotionCraft.Server wwwroot). Нужен CORS на PotionCraft.Server + отдельный systemd unit + деплой-скрипт по аналогии с deploy/ps-scripts/deploy-client.ps1 и deploy/service-files/potioncraft-client.service, но для статики (например через `serve`/`vite preview`/nginx).
- PotionCraft.Client (Blazor) НЕ удаляется — остаётся как референс/бэкап рядом, пока новый фронтенд не пройдёт приёмку. В самом конце (отдельным шагом, после подтверждения пользователем) можно вывести из деплоя.
- Новый проект: `PotionCraft.Web/` в корне репозитория (рядом с PotionCraft.Client).

## Найденная архитектура (справка)
- 8 страниц Blazor: Index (Home), Characters/Select, Characters/Create, Gathering/Index, Alchemy/Index, Herbs/Index, Shop, SourceAndIdea, Error.
- 13 API endpoints на PotionCraft.Server (Characters, Gathering, Herbs, Alchemy, Shop) — см. полный список в отчёте ниже.
- Сессия сейчас держится в sessionStorage: `player_session_id` (UUID) + `active_character` (JSON). Это нужно перенести 1:1 в Zustand + localStorage/sessionStorage persist.
- Ключевая клиентская логика для переноса: `wwwroot/js/shop.js` (калькуляция цен, зеркало серверного `IPriceCalculator`), кассадные селекты в Alchemy/Index, клиентская фильтрация/сортировка в Herbs/Index.
- Контракты/enum описаны в PotionCraft.Contracts (HerbTypeEnum [Flags], RarityEnum, TerrainEnum, PotionTypeEnum, DiceTypeEnum, DamageTypeEnum, HerbModifierEffectEnum, AdditionalRulesEnum, RollResultEnum) — их нужно продублировать как TS enum/union types.

### Полный список API endpoints (переносятся 1:1, backend не переписывается, только добавляется CORS)
```
GET    /api/characters
POST   /api/characters
POST   /api/characters/{id}/select
POST   /api/characters/{id}/deselect
GET    /api/characters/{id}/bag
POST   /api/characters/{id}/bag/coins
POST   /api/gathering/{characterId}
GET    /api/herbs
POST   /api/alchemy/resolve
GET    /api/shop/inventory
POST   /api/shop/trade
```

---

## Steps

### 1. Скелет сайта (инфраструктура)
Создать `PotionCraft.Web/` через Vite (`react-ts` template). Настроить:
- TypeScript strict, ESLint + Prettier.
- Tailwind CSS (postcss.config, tailwind.config с базовой темой/цветами под "гербализм/алхимия").
- React Router v6: маршруты `/`, `/characters/select`, `/characters/create`, `/gathering`, `/alchemy`, `/herbs`, `/shop`, `/source-and-idea`, `*` (404/Error).
- Layout-компонент (`AppLayout`) с navbar (ссылки на все разделы), badge активного персонажа с popover/dropdown, offcanvas/drawer сумки, footer — аналог `_Layout.cshtml`.
- API-клиент: `src/api/client.ts` — обёртка над `fetch`, базовый URL из `import.meta.env.VITE_API_BASE_URL`, типизированные функции по каждому из 11 endpoints (`src/api/characters.ts`, `gathering.ts`, `herbs.ts`, `alchemy.ts`, `shop.ts`).
- TS типы/enum в `src/types/` — 1:1 копии моделей и enum из PotionCraft.Contracts (PlayerCharacter, Tool, CharacterBag, Herb, GatheringResult, Potion, ShopItem, TradeRequest/Result, и все enum).
- Zustand store `src/store/sessionStore.ts`: `playerId` (генерируется `crypto.randomUUID()` при первом запуске, persist в localStorage), `activeCharacter` (persist), `setActiveCharacter`, `clearActiveCharacter`.
- React Query `QueryClientProvider` в `main.tsx`.
- CORS на backend: в `PotionCraft.Server/Program.cs` добавить `AddCors` + `UseCors` с origin нового фронтенда (dev: `http://localhost:5173`, prod: домен из конфига — вынести в appsettings как `AllowedCorsOrigin`).
- `.env`/`.env.production` с `VITE_API_BASE_URL`.
- Деплой: новый systemd unit + ps-скрипт по аналогии с `deploy/service-files/potioncraft-client.service` и `deploy/ps-scripts/deploy-client.ps1`, раздающие `dist/` (через `serve` или nginx) — оформить отдельным шагом после того как приложение готово (Phase "Деплой", в конце).

**Промт для реализации:**
> Создай новый проект `PotionCraft.Web` в корне репозитория PotionCraft на Vite + React + TypeScript. Настрой Tailwind CSS, ESLint, Prettier, React Router v6 (маршруты: /, /characters/select, /characters/create, /gathering, /alchemy, /herbs, /shop, /source-and-idea, catch-all). Сделай layout-компонент AppLayout с навигацией по всем разделам, badge активного персонажа (popover) и offcanvas сумкой — по образцу PotionCraft.Client/Pages/Shared/_Layout.cshtml. Добавь API-клиент (fetch-обёртка, базовый URL из VITE_API_BASE_URL) с типизированными функциями для всех endpoints PotionCraft.Server/Controllers (Characters, Gathering, Herbs, Alchemy, Shop — список путей ниже). Продублируй в src/types/ все модели и enum из PotionCraft.Contracts (PlayerCharacter, Tool, CharacterBag, Herb, GatheringResult, Potion, ShopItem, TradeRequest, TradeResult, HerbTypeEnum, RarityEnum, TerrainEnum, PotionTypeEnum, DiceTypeEnum, DamageTypeEnum, HerbModifierEffectEnum, AdditionalRulesEnum, RollResultEnum). Создай Zustand store для playerId (UUID, генерируется один раз, persist в localStorage) и activeCharacter (persist). Подключи React Query provider. В PotionCraft.Server/Program.cs добавь CORS policy для origin нового фронтенда (значение из конфигурации appsettings, ключ AllowedCorsOrigin). Не удаляй и не трогай PotionCraft.Client — он остаётся как референс. Endpoints: GET/POST /api/characters, POST /api/characters/{id}/select, POST /api/characters/{id}/deselect, GET /api/characters/{id}/bag, POST /api/characters/{id}/bag/coins, POST /api/gathering/{characterId}, GET /api/herbs, POST /api/alchemy/resolve, GET /api/shop/inventory, POST /api/shop/trade.

---

### 2. Страница «Главная» (Home)
*Зависит от шага 1.*
Простая приветственная страница с 2 кнопками: "Выбрать персонажа" → `/characters/select`, "Создать персонажа" → `/characters/create`. 1:1 копия текста/смысла из `Index.cshtml`.

**Промт:**
> Реализуй страницу Home (`src/pages/HomePage.tsx`) в PotionCraft.Web — приветственный экран с текстом и двумя кнопками навигации: "Выбрать персонажа" (на /characters/select) и "Создать персонажа" (на /characters/create). Возьми текст/тон из PotionCraft.Client/Pages/Index.cshtml. Оформи в Tailwind, используй AppLayout.

---

### 3. Страница «Выбор персонажа» (Characters/Select)
*Зависит от шага 1. Может выполняться параллельно со страницей 4.*
Список персонажей (карточки) через `GET /api/characters` (React Query). Показывать характеристики (Интеллект, Мудрость, Бонус мастерства, модификаторы инструментов). Цветовая индикация: зелёная граница = свободен, серая = занят другим (`SelectedBy` ≠ null и ≠ мой playerId), синяя = выбран мной. Кнопка "Выбрать" → `POST /api/characters/{id}/select` с `{playerId}` из store, обработка 409 Conflict (алерт "уже занят"). После успеха — записать `activeCharacter` в Zustand и, вероятно, редиректить или обновлять badge.

**Промт:**
> Реализуй страницу `src/pages/characters/SelectCharacterPage.tsx` — список персонажей с карточками (Интеллект, Мудрость, Бонус мастерства, модификаторы инструментов травник/алхимик/отравитель). Данные — `GET /api/characters` через React Query. Цвет карточки: зелёный (свободен), серый (занят другим playerId), синий (выбран текущим playerId — из Zustand sessionStore). Кнопка "Выбрать" вызывает `POST /api/characters/{id}/select` с телом `{ playerId }`; при 409 показать сообщение "персонаж уже занят"; при успехе — сохранить персонажа в sessionStore (Zustand) и инвалидировать кэш списка персонажей. Логика 1:1 с PotionCraft.Client/Pages/Characters/Select.cshtml(.cs).

---

### 4. Страница «Создание персонажа» (Characters/Create)
*Зависит от шага 1. Параллельно со шагом 3.*
Форма: Имя, Интеллект (0-100), Мудрость (0-100), Бонус мастерства (2-6), и для каждого из 3 инструментов (Травник/Алхимик/Отравитель) — радио Нет/Владение/Мастерство + числовой модификатор (-3..+3). Валидация через React Hook Form + Zod. Сабмит → `POST /api/characters`, после успеха — редирект на Select (или сразу выбрать созданного персонажа).

**Промт:**
> Реализуй страницу `src/pages/characters/CreateCharacterPage.tsx` с формой создания персонажа: поля Имя, Интеллект (0-100), Мудрость (0-100), Бонус мастерства (2-6), и для трёх инструментов (Травник/Алхимик/Отравитель) — радио-группа "Нет/Владение/Мастерство" + числовой модификатор от -3 до 3. Используй React Hook Form + Zod для валидации границ значений (аналогично ограничениям в PotionCraft.Client/Pages/Characters/Create.cshtml.cs и модели PlayerCharacter/Tool). По сабмиту вызови `POST /api/characters`, при успехе перейди на страницу выбора персонажей.

---

### 5. Страница «Сбор трав» (Gathering)
*Зависит от шага 1 (и от наличия activeCharacter в store — предусмотреть redirect на Select, если персонаж не выбран).*
Форма параметров: День/Ночь, Дождь/Нет, Пещера/Нет, Провизия Да/Нет (радио), Сложность поиска (число), Количество бросков (число), Среда обитания (select из TerrainEnum). Кнопка "Бросок на сбор трав" → `POST /api/gathering/{characterId}` с телом параметров. Результат: список бросков d20+модификатор, число успехов, таблица собранных трав, кнопка "копировать в буфер обмена" (Clipboard API).

**Промт:**
> Реализуй страницу `src/pages/GatheringPage.tsx`. Форма параметров сбора трав: радио День/Ночь, Дождь/Не дождь, Пещера/Не пещера, Провизия Да/Нет; числовые поля Сложность и Количество бросков; select Среда обитания (enum TerrainEnum). Если activeCharacter не выбран в sessionStore — показать сообщение/редирект на /characters/select. По кнопке "Бросок на сбор трав" вызови `POST /api/gathering/{characterId}` с телом `{ terrain, isRaining, isNight, isCave, hasProvisions, rollsCount, difficulty }` (используй React Query mutation). Отобрази результат: массив бросков d20, общее число успехов, таблицу собранных трав (herbId → количество, подставляя названия трав из `GET /api/herbs`), и кнопку копирования результата в буфер обмена (navigator.clipboard). Логика 1:1 с PotionCraft.Client/Pages/Gathering/Index.cshtml(.cs).

---

### 6. Страница «Алхимия» (Alchemy)
*Зависит от шага 1 и от загруженного списка трав (шаг использует те же данные, что и Herbs — можно переиспользовать общий хук `useHerbs()`).*
Самая сложная страница: выбор базовой травы (select, фильтр только HealingBase/PoisonBase), до 3 модификаторов (кнопка "Добавить" открывает новый select с релевантными модификаторами по типу базовой травы), чекбокс "Только содержимое сумки" (фильтрует по CharacterBag.Herbs, нужен `GET /api/characters/{id}/bag`). Итог — вызов `POST /api/alchemy/resolve` с `{baseHerbId, modifierHerbIds[]}`, отображение готового `FormulaText` с сервера **без** пересборки на клиенте (важно: не дублировать серверную логику формулы — см. заметку в памяти репозитория).

**Промт:**
> Реализуй страницу `src/pages/AlchemyPage.tsx`. Загрузи все травы через `GET /api/herbs` (общий хук `useHerbs()`, переиспользуемый также страницей Herbs). Селект базовой травы — фильтр только по HerbTypeEnum.HealingBase или PoisonBase. До 3 дополнительных select для модификаторов, релевантных типу базовой травы (HealingModifier/PoisonModifier), кнопка "Добавить" добавляет очередной select. Чекбокс "Только содержимое сумки" — если включен и есть activeCharacter, подгрузи `GET /api/characters/{id}/bag` и фильтруй травы по наличию в CharacterBag.Herbs. При изменении выбора вызывай `POST /api/alchemy/resolve` с `{ baseHerbId, modifierHerbIds }` (порядок массива = порядок добавления) и отображай `PotionEffectResult.FormulaText` как есть, БЕЗ пересборки текста формулы на клиенте — сервер уже формирует готовую строку. Логика 1:1 с PotionCraft.Client/Pages/Alchemy/Index.cshtml(.cs).

---

### 7. Страница «Справочник трав» (Herbs)
*Зависит от шага 1. Может переиспользовать хук `useHerbs()` из шага 6 (порядок реализации 6/7 не важен, можно параллельно).*
Таблица всех трав (`GET /api/herbs`) с клиентской фильтрацией (Название, Описание, Тип, Редкость, Эффект, Сложность, Среда обитания — текстовые/select фильтры) и клиентской сортировкой по кликабельным заголовкам столбцов (с индикатором направления ▲▼).

**Промт:**
> Реализуй страницу `src/pages/HerbsPage.tsx` — таблица всех трав из `GET /api/herbs` (хук `useHerbs()`). Фильтры (все клиентские, без похода на сервер): текстовый поиск по Названию и Описанию, select по HerbTypeEnum, select по RarityEnum, текстовый поиск по Эффекту, числовой фильтр по Сложности, select по TerrainEnum (среда обитания, с учётом Herb.Habitats как словаря terrain→сложность). Кликабельные заголовки столбцов включают сортировку (по возрастанию/убыванию, с иконкой направления) по Названию, Описанию, Типу, Редкости, Сложности, Эффекту, Среде обитания. Логика 1:1 с PotionCraft.Client/Pages/Herbs/Index.cshtml(.cs).

---

### 8. Страница «Магазин» (Shop)
*Зависит от шага 1. Самая сложная логика состояния — рекомендую делать последней среди страниц.*
3-колоночный layout: слева ассортимент торговца (`GET /api/shop/inventory`, фильтр Все/Травы/Зелья/Яды), в центре — текущая корзина сделки (добавленные к покупке/продаже позиции с количеством), справа — инвентарь персонажа (`GET /api/characters/{id}/bag`). Нужно перенести 1:1 логику калькуляции цен из `wwwroot/js/shop.js` (getPriceDecayRate, getAlpha, getDemandFactor/getSupplyFactor, calcBuyTotal/calcSellTotal) в TS-модуль — **зеркало серверного IPriceCalculator**, используется только для предпросмотра, окончательная сумма подтверждается сервером в ответе `TradeResult`. Кнопка "Совершить сделку" → `POST /api/shop/trade` с `{characterId, itemsToBuy[], itemsToSell[]}`, после успеха обновить сумку и показать `TradeResult.Message`.

**Промт:**
> Реализуй страницу `src/pages/ShopPage.tsx` с 3-колоночным layout (Tailwind grid): слева — ассортимент из `GET /api/shop/inventory` с фильтром по категории (herb/potion/poison), в центре — корзина текущей сделки (список добавленных к покупке и продаже позиций с количеством и промежуточной суммой), справа — инвентарь персонажа из `GET /api/characters/{id}/bag`. Перенеси в `src/lib/priceCalculator.ts` логику расчёта цен 1:1 из PotionCraft.Client/wwwroot/js/shop.js (getPriceDecayRate, getAlpha, getDemandFactor, getSupplyFactor, calcBuyTotal, calcSellTotal) — это клиентское превью цены, финальная сумма и валидация всегда приходят от сервера. По кнопке подтверждения вызови `POST /api/shop/trade` с `{ characterId, itemsToBuy, itemsToSell }`, при успехе обнови локальное состояние сумки данными из `TradeResult.UpdatedBag`, покажи `TradeResult.Message`; при неуспехе покажи ошибку без изменения состояния. Логика 1:1 с PotionCraft.Client/Pages/Shop.cshtml(.cs).

---

### 9. Прочие статические страницы + Error/404
*Параллельно с любым шагом после 1.*
`SourceAndIdea` — статический информационный контент (1:1 копия текста). Общий `ErrorPage`/`NotFoundPage` для React Router error boundary и catch-all маршрута (замена `Error.cshtml`).

**Промт:**
> Реализуй `src/pages/SourceAndIdeaPage.tsx` (статический текст, 1:1 копия из PotionCraft.Client/Pages/SourceAndIdea.cshtml) и `src/pages/NotFoundPage.tsx`/error boundary для React Router (замена PotionCraft.Client/Pages/Error.cshtml) — показывает сообщение об ошибке и (если доступно) request id/детали.

---

### 10. Деплой нового фронтенда (в конце, после приёмки всех страниц)
Добавить артефакт публикации (`vite build` → `dist/`), новый systemd unit (например `potioncraft-web.service`, статика через `serve` или nginx conf) и ps-скрипт `deploy-web.ps1` по аналогии с `deploy/ps-scripts/deploy-client.ps1` + новую task в `.vscode/tasks.json`. Обновить `deploy/deploy.config.psd1` при необходимости. PotionCraft.Client (Blazor) и его systemd unit оставить как есть — вывод из продакшена делается отдельным явным решением пользователя, не в рамках этого плана.

**Промт:**
> Добавь публикацию/деплой для PotionCraft.Web: task в .vscode/tasks.json для `npm run build`, systemd unit-файл `deploy/service-files/potioncraft-web.service` (раздача dist/ через `serve` или nginx), ps-скрипт `deploy/ps-scripts/deploy-web.ps1` по образцу deploy-client.ps1. Не трогай существующие unit/скрипты для PotionCraft.Client и PotionCraft.Server.

---

## Verification (после каждой страницы)
1. `npm run build` в PotionCraft.Web без TS-ошибок.
2. `npm run lint` (ESLint) чисто.
3. Ручная проверка в браузере (dev-сервер Vite) при поднятом PotionCraft.Server (dotnet run) — сверить сетевые запросы в DevTools с ожидаемыми endpoints и телами.
4. Для страниц с бизнес-логикой (Alchemy, Shop, Gathering) — сверка результата с текущим Blazor-клиентом бок о бок (одинаковые входные данные → одинаковый результат).
5. CORS: убедиться, что `dotnet test`/существующие C# тесты (`PotionCraft.Tests`) остаются зелёными — новые изменения не должны затрагивать backend-логику, только добавление CORS middleware.

## Decisions
- Backend (PotionCraft.Server, Contracts, Repository) не переписывается — только добавляется CORS. Вся бизнес-логика (цены, сбор трав, алхимия) остаётся на сервере, фронтенд её не дублирует (кроме client-preview калькулятора цен в магазине, который уже так работал и раньше).
- PotionCraft.Client (Blazor) остаётся в репозитории как референс/бэкап, не удаляется и не изменяется в рамках этого плана.
- Порядок страниц (2-9) в основном независим друг от друга (все зависят только от шага 1) — можно распараллелить между сессиями/агентами, кроме Herbs(7)/Alchemy(6), которые логично делать вместе из-за общего хука useHerbs.
