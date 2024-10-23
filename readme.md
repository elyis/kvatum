# Kvatum

## Installation

Для установки проекта необходимо выполнить следующие шаги:

1. Перейти в каждый проект, кроме `startup`, и выполнить скрипт `build.sh`:
    ```bash
    ./build.sh
    ```

2. После создания всех образов перейти в директорию `kvatum-startup` и запустить контейнеры:
    ```bash
    docker compose up -d
    ```

## Environment setup (.env)

Настройка окружения в файле `.env` в директории `kvatum-startup`:

- **ASPNETCORE_ENVIRONMENT**: Установите `"Development"` для включения Swagger или `"Production"` для его отключения.
    ```plaintext
    ASPNETCORE_ENVIRONMENT="Development"
    ```

- **FILE_SERVER_URL**: Внешний URI для доступа к файловому серверу.
    ```plaintext
    FILE_SERVER_URL="http://localhost:8080"
    ```

## Swagger API Documentation

Ниже представлены ссылки на Swagger документацию для каждого из сервисов. Swagger позволяет вам просматривать описание API, выполнять тестовые запросы и узнавать структуру данных, которые используются в сервисах.

- **Auth Service**: Документация для сервиса аутентификации.
  - `/auth/swagger`

- **Chatflow Service**: Документация для сервиса работы с чат, серверами и пространствами.
  - `/chatflow/swagger`

- **File Service**: Документация для сервиса работы с файлами.
  - `/file/swagger`

