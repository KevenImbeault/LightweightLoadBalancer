# Lightweight load balancer
## By Keven Imbeault

_French will follow_  

**Language used** : C#  
**Nuget package used** : Tomly

### API Servers  

---

**Configuration file** : configuration.toml
```toml
[servers]
ips = ["localhost", "localhost", "localhost", "localhost"]
ports = [80, 81, 82, 83]
```  

**_ips_** : Contains the ips of the 1 - 10 servers used as API servers to load balance.  
**_ports_** : Contains the ports to access each server defined previously.

---
#### Required endpoints  

`http:[ip]:[port]/`  
Return code : 200 OK  
Message can be anything, only required to make requests on for testing.

`http:[ip]:[port]/alive`  
Return code : 200 OK  
Used to check wether or not the server being queried is alive or not.

### Load balancing algorithm  

---

This load balancing code is using the "Least Connection" algorithm.  
Load balancer will keep in memory the amount of connections currently being handled by each backend, and send next request to the one with the least amount of requests.  

Using this algorithm enables the usage of API backends with differences in CPU/Memory ressources. 
Since servers with lower ressources will take longer to handle requests, connections will accumulate faster than other servers.
Making it so stronger servers get more traffic, and weaker servers don't get overloaded.

### Testing

---

Program was tested with Node.JS express backend.  
Requests were sent using [Locust](https://locust.io/)  

#### Results  
According to test results, program seems stable at 10000 users and 600-1000 requests per seconds.  

[logo]: Locust.png
![Locust test results][logo]

When a server is lost, around 60 requests fails.

# Load balancer léger
## Par Keven Imbeault

**Language utilisé** : C#  
**Paquet NuGet utilisé** : Tomly

### Serveurs API

---

**Fichier de configuration** : configuration.toml  
```toml
[servers]
ips = ["localhost", "localhost", "localhost", "localhost"]
ports = [80, 81, 82, 83]
```  

**_ips_** : Contient les addresses ips des 1 à 10 serveurs requis pour l'utilisation de l'API.  
**_ports_** : Contient les ports associés aux serveurs défini précédemment.

---
#### Endpoints requis

`http:[ip]:[port]/`  
Code retourné : 200 OK  
Le message n'est pas important, l'endpoint est seulement là pour avoir un endpoint exposé pour les requêtes.

`http:[ip]:[port]/alive`  
Code retourné : 200 OK  
Utilisé pour valider si un serveur est encore en vie, ne seras pas exposé par le load balancer.

### Algorithme de Load Balancing

---
Le code du programme utilise l'algorithme de load balancing "Least Connection".
Le Load balancer va garder en mémoire le nombre de connections présentement géré par chaque serveurs en backend, et envoyé la prochaine requête au serveur qui en gère le moins.

L'utilisation de cet algorithme permet d'utiliser des serveurs n'ayant pas les même ressources au niveau mémoire ou CPU.  
Étant donné que les serveurs avec moins de ressources vont également être plus lent, leur compte de connections va monter plus rapidement et il receverons donc plus de connections.  
De cette manière, les serveurs plus rapide vont recevoir plus de traffic comparément à ceux plus faible.

### Tests

---

Le programme à été tester avec une backend utilisant Node.JS et express.  
Les requêtes ont été envoyé en utilisant [Locust](https://locust.io/)

#### Résultats
Selon les résultats des tests Locust, le programme semble pouvoir garder de manière stable 10000 utilisateurs avec 600 à 1000 requêtes par secondes.

[logo]: Locust.png
![Locust test results][logo]

When a server is lost, around 60 requests fails.
