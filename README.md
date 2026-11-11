# Новичок (MVP)

Гипотеза: часть трудностей новичков можно заметить в первые недели и устранить до того, как они накопятся.

- Склад
- Первая неделя (3 задачи)
- Опрос на 7-й день
- Обращения: «Нужна помощь» → «В работе» → «Решено»

## Стек

ASP.NET Core Razor Pages (.NET 10) + SQLite (`novichok.db`).

## Docker

Из корня репозитория (рядом с `TestNewPeople.sln`):

```bash
docker build -t novichok .
docker run --rm -p 8080:8080 -v novichok-data:/app/data novichok
```

Откройте http://localhost:8080  
База сохраняется в volume `novichok-data`.