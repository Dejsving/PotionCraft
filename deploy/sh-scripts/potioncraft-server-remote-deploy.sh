set -e

mkdir -p '/opt/PotionCraft.Server'

sudo tar -xzf /tmp/potioncraft-server-linux-x64.tar.gz -C '/opt/PotionCraft.Server'
sudo chown -R 'artur':'artur' '/opt/PotionCraft.Server'

sudo find '/opt/PotionCraft.Server' -type d -exec chmod 755 {} +

sudo find '/opt/PotionCraft.Server' -type f -exec chmod 644 {} +

sudo chmod 755 '/opt/PotionCraft.Server/PotionCraft.Server'

cmp -s /tmp/potioncraft-server.service /etc/systemd/system/potioncraft-server.service || sudo cp /tmp/potioncraft-server.service /etc/systemd/system/potioncraft-server.service

sudo systemctl daemon-reload
sudo systemctl enable potioncraft-server.service
sudo systemctl restart potioncraft-server.service

for i in $(seq 1 30); do
    state=$(sudo systemctl is-active potioncraft-server.service 2>/dev/null || true)
    if [ "$state" = "active" ]; then
        echo 'SERVICE_STATUS: active'
        exit 0
    fi
    if [ "$state" = "failed" ]; then
        sudo systemctl status potioncraft-server.service --no-pager --lines=40
        exit 1
    fi
    sleep 1
done

sudo systemctl status potioncraft-server.service --no-pager --lines=40
exit 1