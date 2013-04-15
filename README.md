#AsyncNamedPipes

Librairie permettant d'utiliser les pipes nommées en aynchrone.

##Pourquoi ?

Lors de mon expérience professionnelle, j'ai été amené à utiliser les services WCF en .NET (3.5 / 4.0) pour établir une communication inter-process en mode duplex (entrant/sortant client/serveur).

Hélàs le WCF s'est vite montré insatisfaisant de par les points suivants :
* La documentation reste pauvre et peu compréhensible même sur MSDN.
* Le paramétrage depuis le fichier de config est très difficile pour les novices.
* Services instable lors d'une utilisation intensive et longue.
* Parfois bloquant lors d'une utilisation en mode duplex.

Les pipes nommées corrigent les défauts WCF même si son utilisation de base n'est pas destiné à du full duplex.

##Fonctionnalités

* [ ] Mode Full-Duplex
* [ ] Envoie d'un message vers un client précis ou tous les clients
* [ ] Traitement des messages asynchrone
* [ ] Aucune perte des messages ne pouvant être envoyés (à cause d'une déconnexion, timeout...)
* [ ] Envoie de classe dans le message de communication

##Contribution

* Moi (lanarchyste)



