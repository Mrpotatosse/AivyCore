using AivyData.API.Server.Actor;
using AivyData.API.Server.Look;
using AivyDofus.Protocol.Elements;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.Extension.Server.Data
{
    public static class PlayerDataExtension
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static PlayerData Lua(params object[] args)//long id, string token, long breed, string name, bool sex, long serverId, EntityLookData look)
        {
            PlayerData result = new PlayerData();
            byte reader = 1;
            try
            {
                if (args[reader++] is long id)
                {
                    result.Id = (byte)id;
                }
                if (args[reader++] is string token)
                {
                    result.AccountToken = token;
                }
                if (args[reader++] is long breed)
                {
                    result.Breed = (byte)breed;
                }
                if (args[reader++] is string name)
                {
                    result.Name = name;
                }
                if (args[reader++] is bool sex)
                {
                    result.Sex = sex;
                }
                if (args[reader++] is long serverId)
                {
                    result.ServerId = (int)serverId;
                }
                result.MapId = StaticValues.BREEDS.FirstOrDefault().spawnMap;
                result.CreationDateTime = DateTime.Now;
                if (args[reader++] is EntityLookData look)
                {
                    result.Look = look;
                }
                if(args.Length > reader && args[reader++] is long deathState)
                {
                    result.DeathState = (byte)deathState;
                }
                if (args.Length > reader && args[reader++] is long deathCount)
                {
                    result.DeathCount = (int)deathCount;
                }
                if (args.Length > reader && args[reader++] is long deathMaxLevel)
                {
                    result.DeathMaxLevel = (short)deathMaxLevel;
                }
                return result;
            }
            catch (Exception e)
            {
                logger.Error(e);
                return result;
            }
        }

        public static NetworkContentElement BaseInformation(params object[] args)
        {
            NetworkContentElement result = new NetworkContentElement();
            byte reader = 1;

            try
            {
                if(args[reader++] is PlayerData player)
                {
                    if (args[reader++] is long protocolId)
                        result += player.BaseInformation(protocolId);
                }
                return result;
            }
            catch(Exception e)
            {
                logger.Error(e);
                return result;
            }
        }
        
        public static NetworkContentElement BaseHardcoreInformation(params object[] args)
        {
            NetworkContentElement result = new NetworkContentElement();
            byte reader = 1;

            try
            {
                if (args[reader++] is PlayerData player)
                {
                    if (args[reader++] is long protocolId)
                        result += player.BaseHardcoreInformation(protocolId);
                }
                return result;
            }
            catch (Exception e)
            {
                logger.Error(e);
                return result;
            }
        }

        public static NetworkContentElement BaseInformation(this PlayerData player, long protocolId)
        {
            return new NetworkContentElement()
            {
                fields =
                {
                    { "protocol_id", (int)protocolId },
                    { "id", player.Id },
                    { "name", player.Name },
                    { "level", 1 /*short*/ },
                    { "breed", player.Breed },
                    { "entityLook", player.Look.Look() },
                    { "sex", player.Sex }
                }
            };
        }

        public static NetworkContentElement BaseHardcoreInformation(this PlayerData player, long protocolId)
        {
            NetworkContentElement base_information = player.BaseInformation(protocolId);

            base_information["deathState"] = player.DeathState;// byte -> PlayerLifeStatusEnum
            base_information["deathCount"] = player.DeathCount;// short
            base_information["deathMaxLevel"] = player.DeathMaxLevel;// short

            return base_information;
        }
    }
}
