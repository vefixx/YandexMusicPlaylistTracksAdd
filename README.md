# YandexMusicPlaylistTracksAdd
Скрипт позволяет массово добавлять треки в указанный плейлист. Вы можете передать список ссылок на треки для добавления или указать источник-плейлист, из которого нужно перенести определённое количество треков.
## Конфигурационные файлы и исходные данные
### config.json
```json
{
    "token": "",
    "targetPlaylistUid": ""
}
```

- token - ваш **access_token** аккаунта. Получить его можно по инструкции https://github.com/MarshalX/yandex-music-api/discussions/513
- targetPlaylistUid - uid плейлиста, **куда** нужно перенести треки. Получить его можно из ссылки страницы плейлиста.

### tracks.txt
Файл, в который нужно передать ссылки на **треки**. Каждая ссылка начинается с новой строки
**Структура**
```
url
url
url
```

**Пример:**
```
https://music.yandex.ru/album/27919584/track/118598394?utm_source=web&utm_medium=copy_link
https://music.yandex.ru/album/36148838/track/138018032?utm_source=web&utm_medium=copy_link
https://music.yandex.ru/album/32376881/track/128838466?utm_source=web&utm_medium=copy_link
https://music.yandex.ru/album/26613138/track/115534589?utm_source=web&utm_medium=copy_link
https://music.yandex.ru/album/35411305/track/136144544?utm_source=web&utm_medium=copy_link
https://music.yandex.ru/album/35411305/track/136144545?utm_source=web&utm_medium=copy_link
https://music.yandex.ru/album/35411305/track/136144546?utm_source=web&utm_medium=copy_link
https://music.yandex.ru/album/35411305/track/136144547?utm_source=web&utm_medium=copy_link
https://music.yandex.ru/album/35411305/track/136144548?utm_source=web&utm_medium=copy_link
```

### playlists.txt
Файл, в который нужно передать ссылки на **плейлисты** и количество треков для переноса в указанный плейлист. Структура следующая:
`url;count`

**Пример:**
```
https://music.yandex.ru/users/arzhdmitry/playlists/3?ref_id=369E36A4-DECA-4954-B0F1-B459A302F436&utm_medium=copy_link;30
https://music.yandex.ru/users/arzhdmitry/playlists/3?ref_id=369E36A4-DECA-4954-B0F1-B459A302F436&utm_medium=copy_link;10
https://music.yandex.ru/users/arzhdmitry/playlists/3?ref_id=369E36A4-DECA-4954-B0F1-B459A302F436&utm_medium=copy_link;50
https://music.yandex.ru/users/arzhdmitry/playlists/3?ref_id=369E36A4-DECA-4954-B0F1-B459A302F436&utm_medium=copy_link;32
```

## Принцип работы

Скрипт выполняет обработку в два последовательных этапа:

### Этап 1: Обработка файла `tracks.txt`
1. Скрипт считывает файл и извлекает все ссылки на треки;
2. Из каждого URL с помощью регулярного выражения парсится **`trackId`**;
3. Через Yandex Music API запрашиваются метаданные трека;
4. Трек добавляется в общий контейнер `tracks` для последующего пакетного добавления.

### Этап 2: Обработка файла `playlists.txt`
1. Скрипт считывает файл, парсит ссылки на плейлисты-источники;
2. Из URL извлекаются `username` и `playlistId`;
3. Через API получается список треков из источника;
4. Указанное количество треков (параметр `count`) добавляется в тот же контейнер `tracks`.

### Финальный этап
После завершения обоих этапов все накопленные в контейнере `tracks` треки пакетно добавляются в целевой плейлист (`targetPlaylistUid`).
