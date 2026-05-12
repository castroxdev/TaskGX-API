insert into public."Prioridades" ("ID", "Nome")
values
    (1, 'Baixa'),
    (2, 'Media'),
    (3, 'Alta')
on conflict ("ID") do update
set "Nome" = excluded."Nome";

select setval(pg_get_serial_sequence('public."Prioridades"', 'ID'), coalesce(max("ID"), 1), true)
from public."Prioridades";
