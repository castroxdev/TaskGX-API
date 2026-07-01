create table if not exists public."RecuperacoesSenha" (
    "UsuarioID" integer primary key,
    "CodigoHash" varchar(64) not null,
    "Expiracao" timestamp with time zone not null,
    "TentativasInvalidas" integer not null default 0,
    "CriadoEm" timestamp with time zone not null default now(),
    constraint "FK_RecuperacoesSenha_Usuarios_UsuarioID"
        foreign key ("UsuarioID")
        references public."Usuarios" ("ID")
        on delete cascade
);
