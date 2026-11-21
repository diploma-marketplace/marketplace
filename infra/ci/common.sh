# Спільне для скриптів infra/ci, підключається через source.

# Образ сервісу з compose-файлу: перевірка бере ту саму версію, що й сервер або dev/.
# image_from <compose-файл> <назва образу без тегу>
image_from() {
    local file=$1 name=$2 image
    image=$(docker compose -f "$file" config --images | grep -m1 "^$name:" || true)
    if [[ -z $image ]]; then
        echo "ПОМИЛКА: у $file немає образу $name, перевірка не знає, яку версію брати" >&2
        return 1
    fi
    echo "$image"
}
