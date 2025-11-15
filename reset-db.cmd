@echo off
echo Удаление базы данных...
if exist schedule.db del /f /q schedule.db
if exist schedule.db-shm del /f /q schedule.db-shm
if exist schedule.db-wal del /f /q schedule.db-wal
echo База данных удалена!
echo.
echo При следующем запуске будет создана новая БД с пользователем:
echo Логин: admin
echo Пароль: admin
echo.
pause
