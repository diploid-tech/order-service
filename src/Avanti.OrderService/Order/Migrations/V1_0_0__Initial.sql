CREATE TABLE "order"."order" (
    id serial PRIMARY KEY,
    orderJson jsonb NOT NULL,
    created timestamp with time zone NOT NULL,
    updated timestamp with time zone NOT NULL
);
