Multiagentový systém - klient
=============================

Cílem projektu bylo vytvořit multiagentový systém - piškvorky, kde klienti hrají proti sobě

Projekt se rozděluje do dvou čístí serveru a klintů, kteří komunikují se serverem přes TCP (posílá zprávy a čeká na nové), dále načítá hrací plochu z databáze, kde jsou uloženy i pravidla, které je možné vytvořit v aplikaci a poté uložit v databázi. Dále hledá vhodné pravidlo podle hrací plochy, které lze použít a pošle příkaz serveru, který zobrazuje hrací plochu, komunikuje s klinty, provádí rozlosování, atd...

V projektu byl využit MS SQL server 2008, kde byla uložena potřabná databáze.
