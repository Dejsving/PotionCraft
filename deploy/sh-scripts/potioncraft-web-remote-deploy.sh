set -e

app_dir='/opt/PotionCraft.Web'
staging_dir="$app_dir/dist.next"

sudo mkdir -p "$app_dir"
sudo rm -rf "$staging_dir"
sudo mkdir -p "$staging_dir"
sudo tar -xzf /tmp/potioncraft-web-dist.tar.gz -C "$staging_dir"
sudo chown -R 'artur':'artur' "$staging_dir"
sudo find "$staging_dir" -type d -exec chmod 755 {} +
sudo find "$staging_dir" -type f -exec chmod 644 {} +

if [ ! -x "$app_dir/node_modules/.bin/serve" ]; then
    sudo npm install --prefix "$app_dir" --no-save --no-package-lock serve@14.2.4
fi
sudo chown -R 'artur':'artur' "$app_dir/node_modules"

sudo rm -rf "$app_dir/dist.previous"
if [ -d "$app_dir/dist" ]; then
    sudo mv "$app_dir/dist" "$app_dir/dist.previous"
fi
sudo mv "$staging_dir" "$app_dir/dist"
sudo rm -rf "$app_dir/dist.previous"

sudo chown -R 'artur':'artur' "$app_dir"
cmp -s /tmp/potioncraft-web.service /etc/systemd/system/potioncraft-web.service || sudo cp /tmp/potioncraft-web.service /etc/systemd/system/potioncraft-web.service

sudo systemctl daemon-reload
sudo systemctl enable potioncraft-web.service
sudo systemctl restart potioncraft-web.service

for i in $(seq 1 30); do
    state=$(sudo systemctl is-active potioncraft-web.service 2>/dev/null || true)
    if [ "$state" = 'active' ]; then
        echo 'SERVICE_STATUS: active'
        exit 0
    fi
    if [ "$state" = 'failed' ]; then
        sudo systemctl status potioncraft-web.service --no-pager --lines=40
        exit 1
    fi
    sleep 1
done

sudo systemctl status potioncraft-web.service --no-pager --lines=40
exit 1