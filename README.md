<h3 align="center">SI VOUS N'AVEZ AUCUNE CONNAISSANCE EN PROGRAMMATION, JE NE POURRAIS PAS VOUS AIDER. VEUILLEZ AVANT TOUT VOUS MUNIR D'UNE PETITE BASE EN PROGRAMMATION !</h3>

<p align="center">
  <img src="https://camo.githubusercontent.com/de3e9648ad06c6d749236ad24df6170fd599071f/68747470733a2f2f7062732e7477696d672e636f6d2f6d656469612f456a5f5a656c65585941492d45514e3f666f726d61743d6a7067266e616d653d6d656469756d"/>
</p>

<h3 align="center"> Aivy Core </h3>

<p align="center">C'est gratuit et ça le sera toujours</p>

AivyCore est un program de networking assez basique, qui implémente un Client, un Serveur et un Proxy.
L'intérêt de ce programme n'est pas de vous donnez une base sans même que vous compreniez le fonctionnement, donc, zé partie pour un peu de lecture ( Et lisez tout jusqu'à la fin, sinon si vous n'avez pas le temps ou l'envie, un simple coup d'oeil sur le code-source devrait faire l'affaire. )

<details>
	<summary>Exemple de création d'un Proxy</summary>

```csharp
class Program
{
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
</details>

<h3 align="center"> Aivy Dofus </h3>

AivyDofus est une implémentation de AivyCore pour le jeu Dofus ( www.dofus.com ) ne nécéssitant AUCUNE modification de votre client.

Pour la version compilé vous pouvez le trouver directement sur <a href="https://discord.gg/BrdNtjagf7">Discord</a>.

La configuration du proxy se trouve dans ./proxy_api_information.json (il sera crée automatiquement lors du premier lancement MAIS VIDE !)

<details>
	<summary>Exemple de configuration</summary>
	
```json
[
	{
	    "Name": "default",
	    "FolderPath": "D:\\AppDofus",
	    "ExeName": "Dofus",
	    "Type": 2,
	    "HookRedirectionIp": "127.0.0.1"
	}
]
```
Name = nom de votre configuration

FolderPath = Emplacement de votre Dossier App Dofus (celui qui contiendra l'éxécutable)

ExeName = Nom de votre fichier éxécutable sans l'extension .exe

Type = 0 -> sans type les packets seront directement transmis au serveur
       1 -> dofus retro ( pour l'instant il n'y a que les bases de l'implémentation)
       2 -> dofus 2.XX ( contient la (dé)sérialization des packets )
       
HookRedirectionIp = L'ip vers laquel sera transité tout les packets ( Laissez l'ip locale si vous ne voulez pas faire transitez les packets vers un autre serveur.

⚠ Surtout ne mettez pas les ips des serveurs de Dofus, ce n'est clairement pas l'intérêt de cette propriété ⚠ )

Pour lancer un proxy distant, vous devrez lancer AivyDofus sur votre machine distante avec une config avec le type 0. Et sur votre machine locale, vous devrez lancer AivyDofus
avec comme ip, l'ip de votre machine distante. AivyDofus devra être lancer sur le même port sur la machine distante et local.
</details>

<h3 align="center"> Dofus 2 Handler </h3>

Il vous est possible de 'handle' les messages Dofus avec du code C# et/ou Lua.
Les handlers en C# nécessite une compilation pour pouvoir être ajouté.
Les handlers en Lua peuvent être ajouter/modifier durant le runtime directement depuis la console. 
Le protocol en JSON sera crée/mis à jour, lors de l'ouverture de votre premier proxy.

<details>
	<summary>Exemple de Handler en C#</summary>
	
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
</details>


<details>
	<summary>Exemple de Handler en Lua</summary>

```lua
-- no name restrictions
-- args restrictions
function HANDLER(callback, message, message_content)
	-- return true if message will be forwarded
	-- return false if not
	return true
end

-- check if handler already exist then remove exists
if ID ~= nil then proxy_handlers:Remove('ServerSelectionMessage', ID) end
-- adding handler
proxy_handlers:Add('ServerSelectionMessage', HANDLER)
```
</details>

<h3 align="center"> FAQ </h3>
<details>
	<summary><i>Comment lancer le proxy?</i></summary>
Vous devrez compilez le program et le lancer. Puis il suffit d'éxécuter du code Lua.
Voici un exemple de comment lancer un proxy 

```lua
-- Pour éviter de trop écrire dans la console, je vous recommande d'utiliser dofile('emplacement de votre fichier lua')
config = get_config('default') 
proxy = start_proxy_from_config(config, 666)
-- remote_proxy = start_remote_proxy_from_config(config, 666)
```
<a href="https://www.youtube.com/watch?v=FNYT1cn1AmI">Une petit vidéo youtube pour mieux illustrer</a>
</details>

<details>
	<summary><i>Pourquoi il n'y a que très peu de choses qui sont affiché dans la console?</i></summary>
Par default, les logs via NLog ne sont pas affiché, si vous voulez les affichés, il faudra écrire 'log' dans la console. Une fois les logs activé, il faudra redémarrer AivyCore 
pour retirer l'affichages des logs. 
	
⚠ Seul le nom et l'id des messages seront affiché! ⚠ Pour afficher le contenu, vous avez le choix. 
Si vous voulez affiché le contenu d'un message spécifique, utilisez un handler. Sinon RDV dans la class https://github.com/Mrpotatosse/AivyCore/blob/master/AivyDofus/Proxy/Callbacks/DofusProxyClientReceiveCallback.cs à la ligne 168 ajouter cette 
ligne : 

```csharp
logger.Info($"{data_content}");
```
</details>

<details>
	<summary><i>j'ai la flemme wlh</i></summary>
	='( je rajouterai plus tard
</details>

<h3 align="center"> Dépendances </h3>

- NLog

- NewtonSoft Json

- EasyHook ( SocketHook de Nameless https://cadernis.fr/index.php?threads/sockethook-injector-alternative-%C3%A0-no-ankama-dll.2221/page-2#post-24796 celui que j'utilise est une ancienne version auquel j'ai appliqué quelque modification )

- Botofu parser ( https://gitlab.com/botofu/protocol_parser ) ( j'ai directement ajouter le .exe aux ressources ducoup le protocol devrait être parser à chaque ouverture du hook  https://github.com/Mrpotatosse/AivyCore/blob/master/AivyDofus/Protocol/Parser/BotofuParser.cs )
  
- LiteDB (https://www.litedb.org/) ( pour la base de données côté serveur , c'est du NoSQL pour faciliter le stockage d'object ) ( vous pouvez le modifier et importer la base de données qui vous plait https://github.com/Mrpotatosse/AivyCore/blob/master/AivyDofus/Server/API/OpenServerDatabaseApi.cs )

- NLua (https://github.com/NLua/NLua) (pour éxécuter facilement des scripts Lua (alternatives Handler))

(Pour une explication du code , le principe de base reste le même que Botox https://cadernis.fr/index.php?threads/botox-mitm.2551/ )
