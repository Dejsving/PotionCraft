set -e

mkdir -p '/opt/PotionCraft.Client'

sudo tar -xzf /tmp/potioncraft-client-linux-x64.tar.gz -C '/opt/PotionCraft.Client'
sudo chown -R 'artur':'artur' '/opt/PotionCraft.Client'

sudo find '/opt/PotionCraft.Client' -type d -exec chmod 755 {} +

sudo find '/opt/PotionCraft.Client' -type f -exec chmod 644 {} +

sudo chmod 755 '/opt/PotionCraft.Client/PotionCraft.Client'

cmp -s /tmp/potioncraft-client.service /etc/systemd/system/potioncraft-client.service || sudo cp /tmp/potioncraft-client.service /etc/systemd/system/potioncraft-client.service

sudo systemctl daemon-reload
sudo systemctl enable potioncraft-client.service
sudo systemctl restart potioncraft-client.service

for i in $(seq 1 30); do
    state=$(sudo systemctl is-active potioncraft-client.service 2>/dev/null || true)
    if [ "$state" = "active" ]; then
        echo 'SERVICE_STATUS: active'
        exit 0
    fi
    if [ "$state" = "failed" ]; then
        sudo systemctl status potioncraft-client.service --no-pager --lines=40
        exit 1
    fi
    sleep 1
done

sudo systemctl status potioncraft-client.service --no-pager --lines=40
exit 1
