#!/bin/bash
set -e

echo "=== Налаштування брандмауера UFW ==="
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow 22/tcp
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw --force enable

echo "=== Встановлення та налаштування Fail2ban ==="
sudo apt update && sudo apt install -y fail2ban
sudo cp /etc/fail2ban/jail.conf /etc/fail2ban/jail.local
sudo systemctl enable --now fail2ban

echo "=== Безпеку сервера успішно налаштовано! ==="
