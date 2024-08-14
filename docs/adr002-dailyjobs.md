# Как сделать регулярные джобы

Есть класс `MidnightJobBackgroundService<TJob>` на основе `BackgroundService`.
Его нужно зарегать для каждой джобы (см `DailyJobRegistration`

Он при старте спит до примерно полуночи (плюс случайное количество 5–10 секунд, чтобы не было эффекта нагрузки в ровный час). Потом он по очереди пытается запустить свой `IDailyJob`
Запуск джобы это `INSERT` в табличку `DailyJobRuns text Name, text RunId, enum Status` где `RunId` = 2024-05-05
- У этой таблички уникальный индекс на (Name, RunId)
- Если вставка завершилась неудачей, то джоба уже запущена на другом экземпляре, пытайся запустить следующий тип джобы.
- Если джоба отработала успешно меняем статус на успешный.
- Если джоба упала, меняем статус на ERROR
Все действия пишем в лог.

Таким образом:
- Джоба запускается at most once
- Разные джобы могут быть запущены в параллель на разных экземплярах контейнера или одном
- Джоба может навсегда зависнуть в каком-то статусе (например, быть запущена и не завершится). В этом случае она будет запущена на другой день
- Ответственность джобы идемпотентность (т.е. если она вчера не отработала, должна сделать сегодня за вчера)
- Можно потом добавить механизм ручного перезапуска
- Если контейнер будет штатно выключаться во время работы джобы, у нее 30 секунд, чтобы доделать свою работу