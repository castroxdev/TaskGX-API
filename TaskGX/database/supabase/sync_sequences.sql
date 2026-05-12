select setval(pg_get_serial_sequence('public."Usuarios"', 'ID'), coalesce(max("ID"), 1), true)
from public."Usuarios";

select setval(pg_get_serial_sequence('public."Prioridades"', 'ID'), coalesce(max("ID"), 1), true)
from public."Prioridades";

select setval(pg_get_serial_sequence('public."Listas"', 'ID'), coalesce(max("ID"), 1), true)
from public."Listas";

select setval(pg_get_serial_sequence('public."Tarefas"', 'ID'), coalesce(max("ID"), 1), true)
from public."Tarefas";
