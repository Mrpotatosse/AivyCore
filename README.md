<h3>JE VAIS REECRIRE TOUT LE README , C'EST POUR BIENTOT.</h3>
<h3>I'LL REWRITE ENGLISH VERSION README , IT'S COMING SOON.</h3>

(Pour une explication du code , le principe de base reste le même que Botox https://cadernis.fr/index.php?threads/botox-mitm.2551/ )

English README : https://github.com/Mrpotatosse/AivyCore/blob/master/README_EN.md

<p align="center">
  <img src="https://camo.githubusercontent.com/de3e9648ad06c6d749236ad24df6170fd599071f/68747470733a2f2f7062732e7477696d672e636f6d2f6d656469612f456a5f5a656c65585941492d45514e3f666f726d61743d6a7067266e616d653d6d656469756d"/>
</p>

<h2> AivyCore </h2>

AivyCore est une base de Network contenant un Client , un Server , un Proxy

Le but n'est pas de vous donnez une base pour qu'ensuite vous réutilisiez sans comprendre. Donc je vous invite avant tout , à vous munir d'une base solide en programmation.

<h2> AivyDofus </h2>

AivyDofus est une implémentation de AivyCore pour le jeu Dofus ( www.dofus.com )

La configuration du proxy se trouve dans ./proxy_api_information.json (il sera crée automatiquement MAIS VIDE !)
```json
[
  {
    "Name": "updated",
    "FolderPath": "D:\\AppDofus",
    "ExeName": "Dofus",
    "Type": 2,
    "HookRedirectionIp": "127.0.0.1"
  },  
  {
    "Name": "dofus retro",
    "FolderPath": "D:\\AppDofusRetro\\resources\\app\\retroclient",
    "ExeName": "Dofus",
    "Type": 1,
    "HookRedirectionIp": "127.0.0.1"
  },  
  {
    "Name": "REMOTE PROXY FOR ONLY FORWARD",
    "FolderPath": "",
    "ExeName": "",
    "Type": 0,
    "HookRedirectionIp": "127.0.0.1"
  }
]
```

Lors du lancement un interpréteur Lua ( basé sur NLua ) est lancer pour permettre d'éxécuter du code Lua au runtime ( c'est pour me faciliter les tests pour différents types de
bot , les Handlers dans le code étant trop restrictifs. Les Handlers sont satisfaisant pour des actions globals à éxécuter sur chaque message reçu , pour faire des vérifications
par exemple, mais pour des actions assez rapides dans le jeu ça reste tout de même assez limité pour une compléxité beaucoup trop élevé )

Pour charger un fichier durant le runtime , utilisez la fonction `` dofile('emplacement de votre fichier') ``

<details>
	<summary>Proxy Distant</summary>
	Pour lancer un proxy distant , vous aller devoir lancer un proxy par défault depuis la machine distante (cf. REMOTE PROXY FOR ONLY FORWARD dans l'exemple de config) et un proxy sur votre machine local dont HookRedirection sera l'ip de votre machine distante
	Les 2 proxys (local et distant) devront avoir le même port
	
	lua : start_remote_proxy_from_config() 
	c# : new DofusMultiProxy().RemoteActive() 
</details>


Un exemple d'implémentation en c#

```csharp 
    class Program
    {
        static readonly ConsoleTarget log_console = new ConsoleTarget("log_console");
        static readonly LoggingConfiguration configuration = new LoggingConfiguration();
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static OpenProxyApi _proxy_api;
        static ProxyEntityMapper _proxy_mapper;
        static ProxyRepository _proxy_repository;

        static ProxyCreatorRequest _proxy_creator;
        static ProxyActivatorRequest _proxy_activator;

        static void Main(string[] args)
        {
            configuration.AddRule(LogLevel.Info, LogLevel.Fatal, log_console);
            LogManager.Configuration = configuration;

            _proxy_api = new OpenProxyApi("./proxy_information_api.json");
            _proxy_mapper = new ProxyEntityMapper();
            _proxy_repository = new ProxyRepository(_proxy_api, _proxy_mapper);

            _proxy_creator = new ProxyCreatorRequest(_proxy_repository);
            _proxy_activator = new ProxyActivatorRequest(_proxy_repository);

            ProxyEntity proxy = _proxy_creator.Handle(@"VOTRE FICHIER EXECUTABLE", 666);
            proxy = _proxy_activator.Handle(proxy, true);

            Console.ReadLine();
        }
    }
```

MultiProxy (pour configurer plus facilement des proxys implémentant différent type de handler ) : https://github.com/Mrpotatosse/AivyCore/blob/master/AivyDofus/Proxy/DofusMultiProxy.cs

Proxy : https://github.com/Mrpotatosse/AivyCore/blob/master/AivyDofus/Proxy/DofusProxy.cs

Server : https://github.com/Mrpotatosse/AivyCore/blob/master/AivyDofus/Server/DofusServer.cs

Comme vous pouvez le voir, le proxy gère Dofus 2.0 et Dofus Retro ( cependant pour Dofus Retro aucune lecture des packets reçu n'est implémenté , je n'ai ajouter qu'un petit
script permettant de se connecter sans mettre ses identifiants sur le client (mais directement avec du code) , ça se passe ici : https://github.com/Mrpotatosse/AivyCore/blob/master/AivyDofus/Proxy/Callbacks/DofusRetroProxyClientReceiveCallback.cs 
il fauudra renseigner votre ndc , mdp et la version du jeu)

Voici un exemple pour lancer un DofusServer ou un DofusProxy/DofusRetroProxy

c#

```csharp
    // un exemple pour lancer 10 server
    DofusServer server = new DofusServer("EMPLACEMENT DU DOSSIER APP");
    int nombre_de_server_a_lancer = 10;
    for(int i = 1;i <= nombre_de_server_a_lancer;i++)
    {
        int port_du_server = 666;
        ServerEntity server_instance = server.Active(true, port_du_server + i); 
    }

    // un exemple pour lancer 10 proxy (pour DofusRetro c'est similaire , il suffit de remplacer DofusProxy par DofusRetroProxy et le constructeur de DofusRetroProxy 
    // prend en argument le dossier qui contient le fichier Dofus.exe)
    DofusProxy proxy = new DofusProxy("EMPLACEMENT DU DOSSIER APP");
    int nombre_de_proxy_a_lancer = 10;
    for(int i = 1;i <= nombre_de_proxy_a_lancer;i++)
    {
        int port_du_proxy = 666;
        ProxyEntity proxy_instance = proxy.Active(true, port_du_proxy + i); 
        Thread.Sleep(2000); // le client Dofus peut ne pas se lancer si vous en ouvre plein en même temps donc mettez une pause entre chaque ouverture de client
    }
```

lua 

( les fichiers lua dans https://github.com/Mrpotatosse/AivyCore/blob/master/AivyDofus/Resources/ seront éxéctuer à chaque lancement du program , ils contiendront certaines
fonctionalitées que je rajouterais , pour l'instant il n'y a qu'envoyer des messages , mais je vais remplir au fur et à mesure. )

```lua
-- MAIN PROGRAM --
config = get_config('updated') -- get config
proxy = start_proxy_from_config(config, 666) -- start proxy
accept_callback = multi_proxy[proxy.Port] -- get callback
sleep_then(2000, update_dofus2_protocol) -- async wait 2 sec then update dofus2 protocol
-- END MAIN PROGRAM --



```

<h2> AivyDofus - Dofus 2.0 - Handler </h2>

LE PROTOCOL SOUS FORME DE JSON SE TROUVE DANS LE DOSSIER DE VOTRE EXECUTABLE sous le nom ./protocol.json (lancez le programme 1 fois pour que le fichier se crée automatiquement)

Les Handlers pour Dofus 2.0 sont gérez sous forme de ``class`` 

Proxy Handlers : https://github.com/Mrpotatosse/AivyCore/tree/master/AivyDofus/Proxy/Handlers/Customs

Server Handlers : https://github.com/Mrpotatosse/AivyCore/tree/master/AivyDofus/Server/Handlers/Customs

Voici un exemple de Handler commenté : 

c#

```csharp
    // L'attribut doit être spécifié pour pouvoir handle le message , mettez l'attribut en commentaire si vous voulez désactivez le handle d'un message
    // ProxyHandler pour les proxys et ServerHandler pour les servers
    [ProxyHandler(ProtocolName = "ServerSelectionMessage")]
    // Votre class Handler doit hérité de AbstractMessageHandler https://github.com/Mrpotatosse/AivyCore/blob/master/AivyDofus/Handler/AbstractMessageHandler.cs
    public class ServerSelectionMessageHandler : AbstractMessageHandler
    {
        // optionel pour le log
        static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        // obligatoire , cette variable ne sert que pour le proxy 
        // à TRUE elle redirige directement les données reçu sans aucune modification ( du type du handler ici : ServerSelectionMessage )   
        // à FALSE elle bloque tout les packets reçu ( du type du handler ici : ServerSelectionMessage ) et vous devrez envoyer un message manuellement
        public override bool IsForwardingData => false;

        // le constructeur doit avoir ses arguments la :
        //  - AbstractClientReceiveCallback => _callback : contient
        //             ._tag -> un énum qui définie si le message provient du Client ou du Server
        //             ._client -> qui représente le client ayant activé le callback
        //             ._remote -> le client en lien ( pour le server la valeur est null ) ( pour le proxy , si _tag = Client alors _remote = Server sinon l'inverse )
        //             ._client_repository -> le stockage de tout les clients (à noté que vous pouvez éxécutez des actions depuis cette variable , mais il est préférable de les
        // créer sous forme de class , comme ceux déjà créer , pour éviter tout conflit au niveau de la liste de client )
        //             ._client_creator, ._client_linker, ._client_connector, ._client_disconnector -> differente class qui représente les actions possible sur un client
        //  - NetworkElement => _element : la base du message ( ce qui contient toutes les informations de lecture/écriture )
        //  - NetworkContentElement => _content : le contenu du message reçu
        // Le constructeur ne peux pas être modifié ( sinon il y a aura une erreur lors du runtime )
        public ServerSelectionMessageHandler(AbstractClientReceiveCallback callback,
                                             NetworkElement element,
                                             NetworkContentElement content)
            : base(callback, element, content)
        {

        }
        
        // OBLIGATOIRE , la fonction qui permet de Handle un message
        public override void Handle()
        {
            // Pour créer un message/type il faut passer par un NetworkContentElement
            NetworkContentElement custom_message = new NetworkContentElement()
            {
                field = 
                { "nomDeLaPropriété", null }, // valeur de la propriété
                { "protocol_id" , 0 } // sur certain type , il peut être obligatoire ( dans le protocol c'est si prefixed_by_type_id = true ) 
                // { ... }   
            };
        }
        
        // optionel
        public override void EndHandle()
        {
        
        }
        // optionel
        public override void Error(Exception e)
        {
            logger.Error(e);
        }
    }
```

lua 
```lua

-- AbstractClientReceiveCallback * NetworkElement * NetworkContentElement -> bool
-- return forwarding value (same as IsForwardingData on c# handler source code)
function mon_handler(callback, element, content) -- création du handler
	print('handle '..element.BasicString..' with lua') -- affiche nom et id
	print(message_content) -- affiche le contenu du message
	return true
end

handler_id = handlers:Add('LeNomDuMessageAHandle', mon_handler) -- ajout du handler à la liste des handlers
```

<h2> Dépendances </h2>

- NLog

- NewtonSoft Json

- EasyHook ( SocketHook de Nameless https://cadernis.fr/index.php?threads/sockethook-injector-alternative-%C3%A0-no-ankama-dll.2221/page-2#post-24796 celui que j'utilise est une ancienne version auquel j'ai appliqué quelque modification )

- Botofu parser ( https://gitlab.com/botofu/protocol_parser ) ( j'ai directement ajouter le .exe aux ressources ducoup le protocol devrait être parser à chaque ouverture du hook  https://github.com/Mrpotatosse/AivyCore/blob/master/AivyDofus/Protocol/Parser/BotofuParser.cs )
  
- LiteDB (https://www.litedb.org/) ( pour la base de données côté serveur , c'est du NoSQL pour faciliter le stockage d'object ) ( vous pouvez le modifier et importer la base de données qui vous plait https://github.com/Mrpotatosse/AivyCore/blob/master/AivyDofus/Server/API/OpenServerDatabaseApi.cs )

- NLua (https://github.com/NLua/NLua) (pour éxécuter facilement des scripts Lua (alternatives Handler))
