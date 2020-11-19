using AivyData.API.Server.Look;
using AivyDofus.Protocol.Elements;
using NLog;
using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AivyDofus.Extension.Server.Data
{
    public static class LookExtension
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static SubEntityLookData LuaSubLook(params object[] args)//long bindingPointCategory, long bindingPointIndex, EntityLookData sublook)
        {
            SubEntityLookData result = new SubEntityLookData();
            byte reader = 1;
            try
            {
                logger.Debug(args[reader]);
                if (args[reader++] is long bindingPointCategory)
                {
                    result.BindingPointCategory = (byte)bindingPointCategory;
                }
                logger.Debug(args[reader]);
                if (args[reader++] is long bindingPointIndex)
                {
                    result.BindingPointIndex = (byte)bindingPointIndex;
                }
                logger.Debug(args[reader]);
                if (args[reader++] is EntityLookData sublook)
                {
                    result.SubEntityLook = sublook;
                }

                return result;
            }
            catch(Exception e)
            {
                logger.Error(e);
                return result;
            }
        }

        public static EntityLookData LuaLook(params object[] args)//long bonesId, LuaTable skins, LuaTable indexedColors, LuaTable scales, LuaTable subentities)
        {
            EntityLookData result = new EntityLookData();
            byte reader = 1;
            try
            {
                logger.Debug(args[reader]);
                if (args[reader++] is long bonesId)
                {
                    result.BonesId = (short)bonesId;
                }

                logger.Debug(args[reader]);
                if (args[reader++] is LuaTable skins)
                {
                    result.Skins = new short[skins.Values.Count];
                    int i = 0;
                    foreach(object value in skins.Values)
                    {
                        if (value is long skin)
                            result.Skins[i++] = (short)skin;
                    }
                }

                logger.Debug(args[reader]);
                if (args[reader++] is LuaTable indexedColors)
                {
                    result.IndexedColors = new int[indexedColors.Values.Count];
                    int i = 0;
                    foreach (object value in indexedColors.Values)
                    {
                        if (value is long color)
                            result.IndexedColors[i++] = (int)color;
                    }
                }

                logger.Debug(args[reader]);
                if (args[reader++] is LuaTable scales)
                {
                    result.Scales = new short[scales.Values.Count];
                    int i = 0;
                    foreach (object value in scales.Values)
                    {
                        if (value is long scale)
                            result.Scales[i++] = (short)scale;
                    }
                }

                logger.Debug(args[reader]);
                if (args[reader++] is LuaTable subentities)
                {
                    result.Subentities = new SubEntityLookData[subentities.Values.Count]; 
                    int i = 0;
                    foreach (object value in subentities.Values)
                    {
                        if (value is SubEntityLookData sublook)
                            result.Subentities[i++] = sublook;
                    }
                }

                return result;
            }
            catch(Exception e)
            {
                logger.Error(e);
                return result;
            }
        }
        public static NetworkContentElement Look(this EntityLookData look)
        {
            return new NetworkContentElement()
            {
                fields =
                {
                    { "protocol_id", BotofuProtocolManager.Instance[AivyData.Enums.ProxyCallbackTypeEnum.Dofus2][ProtocolKeyEnum.Types, x => x.name == "EntityLook"] },
                    { "bonesId", look.BonesId },// short
                    { "skins", look.Skins },
                    { "indexedColors", look.IndexedColors },
                    { "scales", look.Scales },
                    { "subentities", look.Subentities.Select(x => x.SubLook()).ToArray() },
                }
            };
        }

        public static NetworkContentElement SubLook(this SubEntityLookData sublook)
        {
            return new NetworkContentElement()
            {
                fields =
                {
                    { "protocol_id", BotofuProtocolManager.Instance[AivyData.Enums.ProxyCallbackTypeEnum.Dofus2][ProtocolKeyEnum.Types, x => x.name == "SubEntity"] },
                    { "bindingPointCategory", sublook.BindingPointCategory }, // byte
                    { "bindingPointIndex", sublook.BindingPointIndex },// byte
                    { "subEntityLook", sublook.SubEntityLook.Look() }
                }
            };
        }

        public static EntityLookData BuildFromRequest(params object[] request)
        {
            for(int i = 0; i < request.Length; i++)
            {
                if(request[i] is NetworkContentElement creation_request)
                {
                    int[] colors = (creation_request["colors"] as dynamic[]).Select(x => x is int v ? v : -1).ToArray();
                    byte breed = creation_request["breed"];
                    bool sex = creation_request["sex"];
                    short cosmeticId = creation_request["cosmeticId"];

                    BreedObjectData breed_data = StaticValues.BREEDS.FirstOrDefault(x => x.Id == breed);
                    HeadObjectData head_data = StaticValues.HEADS.FirstOrDefault(x => x.Id == cosmeticId);

                    return Build(breed_data, head_data, colors, sex);
                }
            }

            return null;
        }

        public static EntityLookData Build(params object[] args)
        {
            EntityLookData result = null;
            byte reader = 1;
            if(args[reader++] is BreedObjectData breed)
            {
                if(args[reader++] is HeadObjectData head)
                {
                    if(args[reader++] is LuaTable colors)
                    {
                        int[] i_colors = new int[colors.Values.Count];
                        int i = 0;
                        foreach (object value in colors.Values)
                            i_colors[i++] = value is long _v ? (int)_v : 0;

                        if (args[reader++] is bool sex)
                        {
                            return breed.Build(head, i_colors, sex);
                        }
                    }
                }
            }
            return result;
        }

        public static EntityLookData Build(this BreedObjectData breed, HeadObjectData head, int[] colors, bool sex)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i] == -1 && (sex ? i < breed.femaleColors.Length : i < breed.maleColors.Length))
                {
                    colors[i] = sex ? breed.femaleColors[i] : breed.maleColors[i];
                }
                colors[i] = (i + 1 & 255) << 24 | colors[i] & 16777215;
            }

            string breed_look = sex ? breed.femaleLook : breed.maleLook;

            string breed_look_pattern = @"(?<bonesId>\d+)\|(?<skin>\d+)\|\|(?<scale>\d+)";

            Match match = Regex.Match(breed_look, breed_look_pattern);

            short bonesId = short.Parse(match.Groups["bonesId"].Value);
            short skin = short.Parse(match.Groups["skin"].Value);
            short scale = short.Parse(match.Groups["scale"].Value);
            
            EntityLookData look = new EntityLookData()
            {
                BonesId = bonesId,
                IndexedColors = colors,
                Skins = new short[] { skin, short.Parse(head.skins) },
                Scales = new short[] { scale },
                Subentities = new SubEntityLookData[0]
            };

            return look;
        }
    }
}
