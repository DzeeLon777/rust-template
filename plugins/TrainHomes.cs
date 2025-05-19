using System.Collections;
using Facepunch;
using Network;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable HeuristicUnreachableCode

namespace Oxide.Plugins
{
    [Info("TrainHomes", "jtedal", "1.1.5")]
    internal sealed class TrainHomes : RustPlugin
    {
        private const bool En = false;

        #region Config

        internal required PluginConfig _config;

        public TrainHomes()
        {
            _config = PluginConfig.DefaultConfig();
        }

        protected override void LoadDefaultConfig()
        {
            Puts("Creating a default config...");
            _config = PluginConfig.DefaultConfig();
            _config.PluginVersion = Version;
            SaveConfig();
            Puts("Creation of the default config completed!");
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            _config = Config.ReadObject<PluginConfig>();
            if (_config.PluginVersion < Version)
            {
                UpdateConfigValues();
            }
        }

        private void UpdateConfigValues()
        {
            Puts("Config update detected! Updating config values...");
            if (_config.PluginVersion < new VersionNumber(1, 0, 1))
            {
                _config.PermissionsForAmountWagons = new Dictionary<string, int>()
                {
                    ["trainhomes.amount1"] = 1,
                    ["trainhomes.amount10"] = 10,
                };
            }
            if (_config.PluginVersion < new VersionNumber(1, 0, 5))
            {
                _config.CheckTopology = false;
            }
            if (_config.PluginVersion < new VersionNumber(1, 0, 7))
            {
                _config.Log = true;
            }
            if (_config.PluginVersion < new VersionNumber(1, 1, 3))
            {
                _config.IsAllowEditWagonOnBase = true;
            }
            _config.PluginVersion = Version;
            Puts("Config update completed!");
            SaveConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_config);
        }

        public sealed class NotifyConfig
        {
            [JsonProperty(En ? "Prefix" : "Префикс сообщений")]
            public string? prefix { get; set; }

            [JsonProperty(En ? "Chat Message setting" : "Настройки сообщений в чате")]
            public ChatConfig? chatConfig { get; set; }

            [JsonProperty(
                En ? "Facepunch Game Tips setting" : "Настройка сообщений Facepunch Game Tip"
            )]
            public GameTipConfig? gameTipConfig { get; set; }

            [JsonProperty(
                En
                    ? "GUI Announcements setting (only for GUIAnnouncements plugin)"
                    : "Настройка GUI Announcements (только для тех, кто использует плагин GUI Announcements)"
            )]
            public GuiAnnouncementsConfig? guiAnnouncementsConfig { get; set; }

            [JsonProperty(
                En
                    ? "Notify setting (only for Notify plugin)"
                    : "Настройка Notify (только для тех, кто использует плагин Notify)"
            )]
            public NotifyPluginConfig? notifyPluginConfig { get; set; }

            [JsonProperty(
                En
                    ? "Discord setting (only for DiscordMessages plugin)"
                    : "Настройка оповещений в Discord (только для тех, кто использует плагин Discord Messages)"
            )]
            public DiscordMessagesConfig? discordMessagesConfig { get; set; }
        }

        public sealed class ChatConfig
        {
            [JsonProperty(
                En ? "Use chat notifications? [true/false]" : "Использовать ли чат? [true/false]"
            )]
            public bool isEnabled { get; set; }
        }

        public sealed class GameTipConfig
        {
            [JsonProperty(
                En
                    ? "Use Facepunch Game Tips (notification bar above hotbar)? [true/false]"
                    : "Использовать ли Facepunch Game Tip (оповещения над слотами быстрого доступа игрока)? [true/false]"
            )]
            public bool isEnabled { get; set; }

            [JsonProperty(
                En
                    ? "Style (0 - Blue Normal, 1 - Red Normal, 2 - Blue Long, 3 - Blue Short, 4 - Server Event)"
                    : "Стиль (0 - Blue Normal, 1 - Red Normal, 2 - Blue Long, 3 - Blue Short, 4 - Server Event)"
            )]
            public int style { get; set; }
        }

        public sealed class GuiAnnouncementsConfig
        {
            [JsonProperty(
                En
                    ? "Do you use GUI Announcements integration? [true/false]"
                    : "Использовать ли GUI Announcements? [true/false]"
            )]
            public bool isEnabled { get; set; }

            [JsonProperty(En ? "Banner color" : "Цвет баннера")]
            public string? bannerColor { get; set; }

            [JsonProperty(En ? "Text color" : "Цвет текста")]
            public string? textColor { get; set; }

            [JsonProperty(En ? "Adjust Vertical Position" : "Отступ от верхнего края")]
            public float apiAdjustVPosition { get; set; }
        }

        public sealed class NotifyPluginConfig
        {
            [JsonProperty(
                En
                    ? "Do you use Notify integration? [true/false]"
                    : "Использовать ли Notify? [true/false]"
            )]
            public bool isEnabled { get; set; }

            [JsonProperty(En ? "Type" : "Тип")]
            public string? type { get; set; }
        }

        public sealed class DiscordMessagesConfig
        {
            [JsonProperty(
                En
                    ? "Do you use DiscordMessages? [true/false]"
                    : "Использовать ли DiscordMessages? [true/false]"
            )]
            public bool isEnabled { get; set; }

            [JsonProperty("Webhook URL")]
            public string? webhookUrl { get; set; }

            [JsonProperty(En ? "Embed Color (DECIMAL)" : "Цвет полосы (DECIMAL)")]
            public int embedColor { get; set; }

            [JsonProperty(En ? "Keys of required messages" : "Ключи необходимых сообщений")]
            public HashSet<string>? keys { get; set; }
        }

        public sealed class AcceptableEntities
        {
            [JsonProperty(nameof(ShortName))]
            public string? ShortName { get; set; }

            [JsonProperty(En ? "Allow this entity to be placed" : "Разрешить ставить этот предмет")]
            public bool IsAllowPut { get; set; }

            [JsonProperty(
                En
                    ? "The maximum amount that can be placed in one wagon (If previous parameter true)"
                    : "Максимальное количество, которое можно ставить в одном вагоне (Если стоит true)"
            )]
            public int Amount { get; set; }
        }

        public sealed class PermissionsConfig
        {
            [JsonProperty(
                En
                    ? "Permission name (Example: trainhomes.name)"
                    : "Название пермишена (Пример: trainhomes.name)"
            )]
            public string? Name { get; set; }
        }

        public sealed class CrateConfig
        {
            [JsonProperty(nameof(Prefab))]
            public string? Prefab { get; set; }

            [JsonProperty(En ? "Chance [0.0-100.0]" : "Шанс выпадения предмета [0.0-100.0]")]
            public float Chance { get; set; }
        }

        public sealed class PluginConfig
        {
            [JsonProperty(En ? "Record plugin logs" : "Записывать логи плагина")]
            public bool Log { get; set; }

            [JsonProperty(
                En
                    ? "Allow to place and edit wagon on foundations"
                    : "Разрешить ставить и редактировать вагон на фундаментах"
            )]
            public bool IsAllowEditWagonOnBase { get; set; }

            [JsonProperty(
                En
                    ? "Allow items to be placed on the roof of the wagon"
                    : "Разрешить ставить предметы на крыше поезда"
            )]
            public bool IsAllowPutOnRoof { get; set; }

            [JsonProperty(
                En
                    ? "Enable topology checking when a player calls a wagon onto the tracks"
                    : "Включить проверку топологий, когда игрок вызывает вагон на рельсы"
            )]
            public bool CheckTopology { get; set; }

            [JsonProperty(
                En
                    ? "The maximum distance at which a wagon can be moved"
                    : "Максимальная дистанция при которой вагон может быть перемещен"
            )]
            public int MaxDistance { get; set; }

            [JsonProperty(En ? "HP of the wagon" : "HP вагона")]
            public int HpWagon { get; set; }

            [JsonProperty(En ? "HP of the wagon stand" : "HP подставки")]
            public int HpBarricade { get; set; }

            [JsonProperty(
                En
                    ? "A constant amount of free wagons on the tracks"
                    : "Постоянное количество бесплатных вагонов на рельсах"
            )]
            public int amountFreeWagons { get; set; }

            [JsonProperty(
                En ? "Wagon spawn settings in crates" : "Настройка появления вагона в ящиках"
            )]
            public HashSet<CrateConfig>? Crates { get; set; }

            [JsonProperty(
                En
                    ? "Whether to use permits for the number of wagons"
                    : "Использовать ли пермишены на кол-во вагонов"
            )]
            public bool AmountWagons { get; set; }

            [JsonProperty(
                En
                    ? "Permissions for amount wagons in using"
                    : "Пермишены на кол-во вагонов для игрока"
            )]
            public Dictionary<string, int>? PermissionsForAmountWagons { get; set; }

            [JsonProperty(En ? "List of entities" : "Список Entity")]
            public HashSet<AcceptableEntities>? AcceptableEntities { get; set; }

            [JsonProperty(En ? "Notification Settings" : "Настройки уведомлений")]
            public NotifyConfig? NotifyConfig { get; set; }

            [JsonProperty(En ? "Configuration Version" : "Версия конфигурации")]
            public VersionNumber PluginVersion { get; set; }

            public static PluginConfig DefaultConfig()
            {
                return new PluginConfig()
                {
                    Log = true,
                    IsAllowEditWagonOnBase = true,
                    IsAllowPutOnRoof = true,
                    CheckTopology = false,
                    MaxDistance = 1500,
                    HpWagon = 5000,
                    HpBarricade = 1000,
                    amountFreeWagons = 5,
                    Crates = new HashSet<CrateConfig>()
                    {
                        new()
                        {
                            Prefab = "assets/bundled/prefabs/radtown/crate_normal.prefab",
                            Chance = 100f,
                        },
                        new()
                        {
                            Prefab = "assets/bundled/prefabs/radtown/crate_elite.prefab",
                            Chance = 50f,
                        },
                    },
                    AmountWagons = false,
                    PermissionsForAmountWagons = new Dictionary<string, int>()
                    {
                        ["trainhomes.amount1"] = 1,
                        ["trainhomes.amount10"] = 10,
                    },
                    AcceptableEntities = new HashSet<AcceptableEntities>()
                    {
                        new()
                        {
                            ShortName = "box.wooden.large",
                            IsAllowPut = true,
                            Amount = 100,
                        },
                    },
                    NotifyConfig = new NotifyConfig
                    {
                        prefix = "[TrainHomes]",
                        chatConfig = new ChatConfig { isEnabled = true },
                        gameTipConfig = new GameTipConfig { isEnabled = false, style = 2 },
                        guiAnnouncementsConfig = new GuiAnnouncementsConfig
                        {
                            isEnabled = false,
                            bannerColor = "Grey",
                            textColor = "White",
                            apiAdjustVPosition = 0.03f,
                        },
                        notifyPluginConfig = new NotifyPluginConfig
                        {
                            isEnabled = false,
                            type = "0",
                        },
                        discordMessagesConfig = new DiscordMessagesConfig
                        {
                            isEnabled = false,
                            webhookUrl =
                                "https://support.discordapp.com/hc/en-us/articles/228383668-Intro-to-Webhooks",
                            embedColor = 13516583,
                            keys = new HashSet<string> { "PreStart", "EventStart", "Finish" },
                        },
                    },
                    PluginVersion = new VersionNumber(),
                };
            }
        }

        #endregion Config

        #region Lang

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(
                new Dictionary<string, string>
                {
                    ["BaseHasntCupboard"] = "{0} You can't do this because there is no cupboard.",
                    ["NotAuthedInCupboard"] =
                        "{0} You can't do this because you're not authorized in the cupboard.",
                    ["NoNeedAmountBlocks"] =
                        "{0} Your base must have at least 21 square foundations. (Details: /thinstruction)",
                    ["NotValidPosForTrainCar"] = "{0} TrainCar cannot be spawned in this position.",
                    ["CanntPutEntityOnRoof"] = "{0} You can't put objects on the roof.",
                    ["CanntPutEntity"] = "{0} You can't put this object on the wagon.",
                    ["SetMaxAmountObjects"] =
                        "{0} You have set the maximum amount of these objects on the wagon.",
                    ["LimitFloorsForHouse"] =
                        "{0} Building blocks cannot be outside the wagon or above the 1st floor.",
                    ["CodeOfTheWagon"] =
                        "{0} Your code from the wagon [{1}]. You can change these parameters.",
                    ["FrequencyOfTheWagon"] =
                        "{0} Your frequency from the wagon [{1}]. You can change this parameter.",
                    ["LongDistance"] =
                        "{0} The distance from the wagon to the spawn point is too long. (Distance: {1}m, Maximum distance: {2}m)",
                    ["TwoCallers"] =
                        "{0} When calling a wagon, the detonator must be in the hand of only 1 player.",
                    ["WagonAlreadyInGarage"] = "{0} The wagon is already in another base.",
                    ["WagonAlreadyOnTracks"] = "{0} The wagon is already on tracks.",
                    ["NotValidTopologies"] =
                        "{0} There are no topologies for spawn in this position. Try to spawn the wagon in another position or contact the server owner.",
                    ["TimerForSpawn"] = "{0} The wagon will spawn in {1}...",
                    ["NotValidConstruction"] =
                        "{0} Incorrect сonstruction for the wagon spawn. (Details: /thinstruction)",
                    ["NotValidTarget"] =
                        "{0} Incorrect сonstruction for the wagon spawn or cannot be spawned in this position. (Подробности: /thinstruction)",
                    ["FoundationsExistInHashSet"] =
                        "{0} You are trying to spawn the wagon in a place where there is already a position for the wagon. (Подробности: /thinstruction)",
                    ["CommandForAdminsOnly"] = "{0} This command is for admins only.",
                    ["RemoveTimeEnd"] = "{0} The time for removing the wagon is over.",
                    ["CantCallAnotherWagon"] = "{0} Wait until the other wagon is spawned.",
                    ["WagonAlreadyCalled"] =
                        "{0} This wagon has already been called by another player.",
                    ["WagonOnMonument"] =
                        "{0} This wagon is located on the custom monument, in building mode. You can't move it.",
                    ["ModeIsNotBuilding"] =
                        "{0} Turn on the monument building mode using the button on the pillar.",
                    ["NotOnBaseAndNotOnMonument"] =
                        "{0} You can't build on a wagon not on base and not on a special custom monument.",
                    ["MuchWagons"] =
                        "{0} To enable building mode, there should not be more than 1 wagon inside the monument.",
                    ["NotCustomWagon"] =
                        "{0} To enable building mode, there should be a custom wagon inside the monument.",
                    ["NotHaveWagons"] =
                        "{0} To enable building mode, there should be a wagon inside the monument.",
                    ["InvalidCmdChat1"] =
                        "{0} You <color=#ce3f27>didn't</color> write amount and player's SteamID! \n/givewagon <amount> <SteamID>",
                    ["InvalidCmdChat2"] =
                        "{0} You <color=#ce3f27>didn't</color> write some player's SteamID! \n/givewagon <amount> <SteamID>",
                    ["InvalidCmdChat3"] =
                        "{0} Player with SteamID: {1} <color=#ce3f27>not found</color>!",
                    ["ValidCmdChat"] = "{0} The wagon was successfully given to the player!",
                    ["YouNotOwner"] =
                        "{0} You can't do this because you are not the owner of the wagon!",
                    ["CantBuildingOnFreeWagon"] =
                        "{0} You can't build on the free wagon! Set the code for the codelock and try again.",
                    ["NotHavePerm"] = "{0} You dont have permission to use this command.",
                    ["NotHavePermForMoreWagons"] =
                        "{0} You do not have perms to own a larger number of wagons.",
                    ["NotValidRaycast"] =
                        "{0} The object has not been found or it does not meet the conditions for recovery the wagon.",

                    ["GUI_General"] = "General",
                    ["GUI_General_Description"] =
                        "1) In the crates, you can find an item for the wagon spawn."
                        + "\n2) On the railway tracks, occasionally, you can find an free wagon. To become its owner, enter any code."
                        + "\n3) You can only place the wagon at your base. For details, see the section 'Construction'."
                        + "\n4) You can build on a wagon only at your base or on a custom monument, if there is one. For more information, see the section 'Custom monument'."
                        + "\n5) Any player can build on the wagon."
                        + "\n6) There is no need to install a cupboard on the wagon, because the objects on it do not decay, as well as the wagon itself."
                        + "\n7) To move the wagon from the base to the rails and back, you will need a detonator. Enter the frequency that is indicated on the wagon, stand in front of the place where you want to move it and press the button."
                        + "\n8) All players who are authorized in the codelock have access to the wagon movement."
                        + "\n9) Any player can change the frequency of the wagon."
                        + "\n10) The owner of the wagon can remove his wagon and get a spawn item in return. Be careful! All objects on it will destroy, and resources will not be returned. (Command: /removewagon)",

                    ["GUI_Construction"] = "Construction",
                    ["GUI_Construction_Description"] =
                        "1) The сonstruction must contain 7x3 foundations."
                        + "\n2) To spawn the wagon, you must definitely put a cupboard."
                        + "\n3) All foundations must be at the same height."
                        + "\n4) If a cupboard, or one of the 21 foundations, or a stand breaks down on the сonstruction while the wagon is located, then the one will also be destroyed."
                        + "\n5) To place a wagon, you must strictly specify the foundation that is the most central of the 21."
                        + "\n6) The wagon must not be placed on foundations other than those.",

                    ["GUI_CustomMonument"] = "Custom Monument",
                    ["GUI_CustomMonument_Description"] =
                        "1) You will not be able to build anything on the wagon until you activate the building mode."
                        + "\n2) The construction mode is activated by pressing a button on one of the pillars."
                        + "\n3) After pressing the button, the wagon will automatically reach the desired position if it is not entirely located in the construction area."
                        + "\n4) Be careful! During the building mode, any player will be able to build anything on your wagon."
                        + "\n5) To exit the building mode, press the button again.",
                },
                this
            );

            lang.RegisterMessages(
                new Dictionary<string, string>
                {
                    ["BaseHasntCupboard"] = "{0} Вы не можете это сделать, потому что нет шкафа.",
                    ["NotAuthedInCupboard"] =
                        "{0} Вы не можете это сделать, потому что не авторизованы в шкафу.",
                    ["NoNeedAmountBlocks"] =
                        "{0} У вашей постройки должно быть минимум 21 квадратных фундаментов. (Подробности: /thinstruction)",
                    ["NotValidPosForTrainCar"] = "{0} В этой позиции нельзя заспавнить вагон.",
                    ["CanntPutEntityOnRoof"] = "{0} Нельзя ставить объекты на крыше.",
                    ["CanntPutEntity"] = "{0} Нельзя ставить этот объект на поезде.",
                    ["SetMaxAmountObjects"] =
                        "{0} Вы установили максимальное количество этих объектов на вагоне.",
                    ["LimitFloorsForHouse"] =
                        "{0} Строительные блоки не могут выходить за пределы одноэтажной конструкции.",
                    ["CodeOfTheWagon"] =
                        "{0} Ваш код от вагона [{1}]. Вы можете изменить этот параметр.",
                    ["FrequencyOfTheWagon"] =
                        "{0} Ваша частота от вагона [{1}]. Вы можете изменить этот параметр.",
                    ["LongDistance"] =
                        "{0} Дистанция от вагона до точки спавна слишком большая. (Дистанция: {1}м, Максимальная дистанция: {2}м)",
                    ["TwoCallers"] =
                        "{0} При вызове вагона, детонатор должен быть в руке только 1 игрока.",
                    ["WagonAlreadyInGarage"] = "{0} Вагон уже находится на другой базе.",
                    ["WagonAlreadyOnTracks"] = "{0} Вагон уже находится на рельсах.",
                    ["NotValidTopologies"] =
                        "{0} В этой позиции нет топологий для спавна. Попробуйте заспавнить вагон в другом месте или обратитесь к владельцу сервера.",
                    ["TimerForSpawn"] = "{0} Вагон появится через {1}...",
                    ["NotValidConstruction"] =
                        "{0} Неправильная конструкция для спавна вагона. (Подробности: /thinstruction)",
                    ["NotValidTarget"] =
                        "{0} Неправильная конструкция или позиция для спавна вагона. (Подробности: /thinstruction)",
                    ["FoundationsExistInHashSet"] =
                        "{0} Вы пытаетесь заспавнить вагон в том месте, где уже есть позиция для вагона. (Подробности: /thinstruction)",
                    ["CommandForAdminsOnly"] = "{0} Эта команда только для админов.",
                    ["RemoveTimeEnd"] = "{0} Время для удаления вагона истекло.",
                    ["CantCallAnotherWagon"] = "{0} Дождитесь пока другой вагон будет заспавнен.",
                    ["WagonAlreadyCalled"] = "{0} Этот вагон уже вызвал другой игрок.",
                    ["WagonOnMonument"] =
                        "{0} Этот вагон находится на монументе, в режиме строитедства. Вы не можете его перемещать.",
                    ["ModeIsNotBuilding"] =
                        "{0} Включите режим строителтсва на монументе через кнопку на столбе.",
                    ["NotOnBaseAndNotOnMonument"] =
                        "{0} Вы не можете строить на вагоне не дома и не на специальном кастомном монументе.",
                    ["MuchWagons"] =
                        "{0} Чтобы включить режим редактирования, внутри монумента не должно быть более 1 вагона.",
                    ["NotCustomWagon"] =
                        "{0} Чтобы включить режим редактирования, внутри монумента должен быть вагон для строительста.",
                    ["NotHaveWagons"] =
                        "{0} Чтобы включить режим редактирования, внутри монумента должен быть вагон.",
                    ["InvalidCmdChat1"] =
                        "{0} Вы <color=#ce3f27>не написали</color> количество и SteamID игрока! \n/givewagon <amount> <SteamID>",
                    ["InvalidCmdChat2"] =
                        "{0} Вы <color=#ce3f27>не написали</color> SteamID игрока! \n/givewagon <amount> <SteamID>",
                    ["InvalidCmdChat3"] =
                        "{0} Игрок с SteamID: {1} <color=#ce3f27>не найден</color>",
                    ["ValidCmdChat"] = "{0} Вагон был удачно выдан игоку!",
                    ["YouNotOwner"] =
                        "{0} Вы не можете это сделать потому что не являетесь владельцем вагона!",
                    ["CantBuildingOnFreeWagon"] =
                        "{0} Строиться на свободном вагоне нельзя! Введите код для кодлока и повторите попытку.",
                    ["NotHavePerm"] = "{0} У вас нет разрешений на использование этой команды.",
                    ["NotHavePermForMoreWagons"] =
                        "{0} У вас нет разрешений на владение большего кол-ва вагонов.",
                    ["NotValidRaycast"] =
                        "{0} Объект не найден или он не соответствует условиям для восстановления вагона.",

                    ["GUI_General"] = "Общее",
                    ["GUI_General_Description"] =
                        "1) В ящиках, можно найти предмет для спавна вагона."
                        + "\n2) На железнодорожных путях, изредка, можно встретить свободный вагон. Чтобы стать его владельцем, введите любой код."
                        + "\n3) Разместить вагон вы можете только у себя на базе. Подробности смотрите в разделе 'Конструкция'."
                        + "\n4) Строиться на вагоне вы можете только у себя на базе или на кастомном монументе, если он есть. Подробности смотрите в разделе ‘Кастом монумент'."
                        + "\n5) Строиться на вагоне может любой игрок."
                        + "\n6) На вагоне не нужно устанавливать шкаф, поскольку объекты на нем не гниют, также как и сам вагон."
                        + "\n7) Для перемещения вагона с базы на рельсы и обратно, вам понадобиться детонатор. Введите частоту, которая указана на вагоне, встаньте перед тем место, куда хотите его переместить и нажмите кнопку."
                        + "\n8) Доступ к перемещению вагона имеют все игроки, которые авторизованы в кодлоке."
                        + "\n9) Частоту вагона может поменять любой игрок."
                        + "\n10) Владелец вагона может удалить свой вагон и взамен получить предмет для спавна. Будьте осторожны! Все объекты на нем пропадут, а ресурсы возвращены не будут. (Команда: /removewagon)",

                    ["GUI_Construction"] = "Конструкция",
                    ["GUI_Construction_Description"] =
                        "1) Конструкция должна содержать 7x3 фундамента."
                        + "\n2) Для размещения вагона вы обязательно должны поставить шкаф."
                        + "\n3) Все фундаменты должны быть на одной высоте."
                        + "\n4) Если во время нахождения вагона на конструкции сломается шкаф, или один из 21 фундамента, или подставка, то вагон также уничтожится."
                        + "\n5) Для размещения вагона, вы должны строго указывать тот фундамент, который самый центральный из 21."
                        + "\n6) Вагон нельзя размещать не на фундаментах.",

                    ["GUI_CustomMonument"] = "Кастом монумент",
                    ["GUI_CustomMonument_Description"] =
                        "1) Вы не сможете что либо построить на вагоне пока не активируете режим строительства."
                        + "\n2) Активация режима строительства происходит путем нажатия кнопки на одном из столбов."
                        + "\n3) После нажатия кнопки, вагон автоматически доедет до нужной позиции, если он целиком не находится в зоне строительства"
                        + "\n4) Будьте осторожны! Во время режима строительства, любой игрок сможет построить что либо на вашем вагоне."
                        + "\n5) Чтобы выйти из режима строительства, нажмите еще раз на кнопку.",
                },
                this,
                "ru"
            );
        }

        private static string GetMessage(string langKey, string userID, params object[] args)
        {
            return (args.Length == 0)
                ? GetMessage(langKey, userID)
                : string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    GetMessage(langKey, userID),
                    args
                );
        }

        #endregion Lang

        #region GUI

        [ConsoleCommand("Close_GUI")]
        private void DestroyGUI(ConsoleSystem.Arg arg)
        {
            GUIManager.DestroyGUI(arg.Player());
        }

        [ConsoleCommand("GUI_General")]
        private void CreateGUI_General(ConsoleSystem.Arg arg)
        {
            GUIManager.CreateGUI(arg.Player(), GUIManager.GUITopic.General);
        }

        [ConsoleCommand("GUI_Construction")]
        private void CreateGUI_Construction(ConsoleSystem.Arg arg)
        {
            GUIManager.CreateGUI(arg.Player(), GUIManager.GUITopic.Construction);
        }

        [ConsoleCommand("GUI_CustomMonument")]
        private void CreateGUI_CustomMonument(ConsoleSystem.Arg arg)
        {
            GUIManager.CreateGUI(arg.Player(), GUIManager.GUITopic.CustomMonument);
        }

        #endregion GUI

        #region Data System

        private void WriteData()
        {
            CreateData();
            Interface.Oxide.DataFileSystem.WriteObject("MM_Data/TrainHomes/Wagons", dataForWagons);
            Interface.Oxide.DataFileSystem.WriteObject(
                "MM_Data/TrainHomes/Garages",
                dataForGarages
            );
        }

        private void LoadFiles()
        {
            dataForWagons = Interface.Oxide.DataFileSystem.ReadObject<HashSet<WagonData>>(
                "MM_Data/TrainHomes/Wagons"
            );
            dataForGarages = Interface.Oxide.DataFileSystem.ReadObject<HashSet<GarageData>>(
                "MM_Data/TrainHomes/Garages"
            );

            LoadDataForWagon();
            LoadDataForGarage();
        }

        private void LoadDataForWagon()
        {
            foreach (WagonData wagonData in dataForWagons)
            {
                Wagon wagon = new();
                bool isValidData = true;
                wagon.InitWithData(wagonData, ref isValidData);

                if (isValidData)
                {
                    _ = wagons.Add(wagon);
                }
                else
                {
                    wagon.RemoveWagon();
                }
            }
        }

        private void LoadDataForGarage()
        {
            foreach (GarageData garageData in dataForGarages)
            {
                BuildingPrivlidge? cupboard =
                    BaseNetworkable.serverEntities.FirstOrDefault(
                        p => p.net.ID.Value == garageData.idCupboard
                    ) as BuildingPrivlidge;

                Garage garage = new();
                garage.InitWithData(garageData);

                if (cupboard == null)
                {
                    garage.DestroyAllWagons();
                    continue;
                }

                if (garage.wagons.Count > 0)
                {
                    dicGarages.Add(cupboard, garage);
                }
            }
        }

        private void CreateData()
        {
            dataForWagons.Clear();
            dataForGarages.Clear();
            CreateDataForWagon();
            CreateDataForGarage();
        }

        private void CreateDataForWagon()
        {
            foreach (Wagon wagon in wagons.ToHashSet())
            {
                if (wagon?.freeWagon)
                {
                    continue;
                }

                WagonData data = wagon.CreateInfoForData();
                _ = dataForWagons.Add(data);
            }
        }

        private void CreateDataForGarage()
        {
            foreach (KeyValuePair<BuildingPrivlidge, Garage> garage in dicGarages)
            {
                GarageData data = garage.Value.CreateInfoForData(garage.Key.net.ID.Value);
                _ = dataForGarages.Add(data);
            }
        }

        #endregion Data System

        #region Oxide Hooks

        private void Init()
        {
            ownIns = this;
            PermissionManager.RegisterPermissions();
        }

        private void Unload()
        {
            UpdateWagonsForUnload();
            WriteData();
            DestroyStandsAndMods();
            DestroyAllCustomMonuments();
            dicGarages.Clear();
            ControllerSpawnsFreeWagon.Unload();
            ownIns = null;
        }

        private void OnServerInitialized()
        {
            if (GUIAnnouncements == null)
            {
                PrintError("GUIAnnouncements plugin is not installed!");
            }

            if (DiscordMessages == null)
            {
                PrintError("DiscordMessages plugin is not installed!");
            }

            if (Notify == null)
            {
                PrintError("Notify plugin is not installed!");
            }

            if (MadMappersPluginUpdate == null)
            {
                PrintError("MadMappersPluginUpdate plugin is not installed!");
            }

            if (ArmoredTrain == null)
            {
                PrintError("ArmoredTrain plugin is not installed!");
            }

            PluginUpdateManager.CheckPluginUpdate();
            LoadFiles();

            DetermineSettingsEntities();

            ControllerSpawnsFreeWagon.Init();
            DetermineAllMonumentsForWagon();
        }

        private void OnServerSave()
        {
            WriteData();
        }

        [HookMethod("OnEntityKill")]
        private object? OnEntityKill(BaseEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            _ = allBaseEntityByWagons.Remove(entity.net.ID.Value);
            if (allStands.TryGetValue(entity.net.ID.Value, out Stand? stand))
            {
                OnRemoveWagon((Stand)stand, entity.GetComponent<BasePlayer>());
            }
            return null;
        }

        [HookMethod("OnWagonKill")]
        private object? OnWagonKill(BaseEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            _ = allParents.Remove(entity.net.ID.Value);
            return null;
        }

        [HookMethod("OnTrainCarKill")]
        private object? OnTrainCarKill(BaseEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            _ = allTrainCars.Remove(entity.net.ID.Value);
            return null;
        }

        [HookMethod("OnBuildingBlockKill")]
        private object? OnBuildingBlockKill(BaseEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            _ = allFoundationsUnderWagons.Remove(entity.net.ID.Value);
            return null;
        }

        [HookMethod("OnCupboardKill")]
        private object? OnCupboardKill(BaseEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            if (entity is BuildingPrivlidge cupboard)
            {
                _ = dicGarages.Remove(cupboard);
            }

            return null;
        }

        private object? OnEntityTakeDamage(BaseCombatEntity baseCombatEntity, HitInfo info)
        {
            if (baseCombatEntity == null || info == null)
            {
                return null;
            }

            if (baseCombatEntity.net == null)
            {
                return null;
            }

            if (_config.Log)
            {
                LogManager.AddLogDamage(baseCombatEntity, info);
            }

            return null;
        }

        private bool? OnEntityTakeDamage(Barricade barricade, HitInfo info)
        {
            if (barricade == null || barricade.net == null)
            {
                return null;
            }

            if (allObjectsOfWalls.Contains(barricade.net.ID.Value))
            {
                return true;
            }

            return null;
        }

        private bool? OnEntityTakeDamage(NeonSign neonSign, HitInfo info)
        {
            if (neonSign == null || neonSign.net == null)
            {
                return null;
            }

            if (allObjectsOfWalls.Contains(neonSign.net.ID.Value))
            {
                return true;
            }

            return null;
        }

        private bool? OnEntityTakeDamage(RFReceiver receiver, HitInfo info)
        {
            if (receiver == null)
            {
                return null;
            }

            if (allReceivers.Contains(receiver))
            {
                return true;
            }

            return null;
        }

        private bool? OnEntityTakeDamage(RANDSwitch randSwitch, HitInfo info)
        {
            if (randSwitch == null)
            {
                return null;
            }

            if (allSwitches.Contains(randSwitch))
            {
                return true;
            }

            return null;
        }

        private bool? CanPickupEntity(BasePlayer player, DoorCloser doorCloser)
        {
            if (doorCloser == null || doorCloser.net == null)
            {
                return null;
            }

            if (doorCloser.skinID != SkindIdFlasher)
            {
                return false;
            }

            return null;
        }

        private object? OnButtonPress(PressButton button, BasePlayer player)
        {
            if (button == null || !player.IsPlayer())
            {
                return null;
            }

            if (buttonsOnMonuments.TryGetValue(button, out CustomMonument? monument))
            {
                monument.TryUpdateState(player);
            }

            return null;
        }

        private void OnLootSpawn(LootContainer container)
        {
            if (container == null || container.inventory == null)
            {
                return;
            }

            CrateConfig? config = _config.Crates.FirstOrDefault(
                x => x.Prefab == container.PrefabName
            );
            if (config == null)
            {
                return;
            }

            if (UnityEngine.Random.Range(0f, 100f) > config.Chance)
            {
                return;
            }

            if (container.inventory.itemList.Count == container.inventory.capacity)
            {
                container.inventory.capacity++;
            }

            Item wagon = ItemManager.CreateByName("electric.flasherlight", 1, SkindIdFlasher);
            wagon.name = "Wagon";
            if (!wagon.MoveToContainer(container.inventory))
            {
                wagon.Remove();
            }
        }

        private bool? CanChangeCode(
            BasePlayer player,
            CodeLock codeLock,
            string newCode,
            bool isGuestCode
        )
        {
            if (allCodeLocks.Contains(codeLock) && codeLock.GetParentEntity() is ModificationsWagon modifications)
            {
                if (!CanHaveMoreWagons(player))
                {
                    return true;
                }

                if (!amountWagons.TryGetValue(player, out int value))
                {
                    value = 0;
                    amountWagons.Add(player, value);
                }

                amountWagons[player] = ++value;
                NextTick(() => modifications.ChangeStateToLocked(player));
            }
            return null;
        }

        private void OnEntitySpawned(TrainCar trainCar)
        {
            NextTick(() => ControllerSpawnsFreeWagon.TryAddNewTrainCar(trainCar));
        }

        #endregion Oxide Hooks

        #region RustEdit Hooks

        private void RustEdit_OnMapDataProcessed()
        {
            DetermineAllMonumentsForWagon();
        }

        #endregion RustEdit Hooks

        #region Helpers

        private static void UpdateWagonsForUnload()
        {
            if (ownIns == null)
            {
                throw new InvalidOperationException("Plugin instance is not initialized");
            }
            foreach (Wagon wagon in ownIns.wagons)
            {
                if (wagon.onCustomMonument)
                {
                    wagon.UpdateForNotBuildingBlocks();
                }
            }
        }

        private static void OnRemoveWagon(Stand stand, BasePlayer player)
        {
            GiveWagon(player, 1);

            if (stand.wagon.owner == player)
            {
                stand.wagon.RemoveWagon();
            }
            else
            {
                NotifyManager.SendMessageToPlayer(
                    player,
                    "YouNotOwner",
                    ownIns._config.NotifyConfig.prefix
                );
            }
        }

        private void CheckBaseEntity(
            BaseEntity baseEntity,
            Construction construction,
            Construction.Target target,
            BasePlayer player
        )
        {
            if (!player.IsPlayer())
            {
                return;
            }

            if (baseEntity == null)
            {
                return;
            }

            if (IsItItemForWagonSpawn(baseEntity))
            {
                if (!CanHaveMoreWagons(player) || !_config.IsAllowEditWagonOnBase)
                {
                    CancelAction(baseEntity, player);
                    return;
                }
                TryCreateNewGarage(baseEntity, target.entity, player);
            }

            if (target.entity == null)
            {
                return;
            }

            if (!IsTargetEntityBelongWagon(target.entity, out Wagon wagon))
            {
                return;
            }

            if (wagon.parent != null)
            {
                wagon.CheckNewEntity(baseEntity, target.entity, player);
            }
            else
            {
                CheckWagonOnCustomMonument(player, baseEntity, target.entity, wagon);
            }
        }

        private static bool IsItItemForWagonSpawn(BaseEntity baseEntity)
        {
            return baseEntity.ShortPrefabName == "electric.flasherlight.deployed"
                && baseEntity.skinID == SkindIdFlasher;
        }

        private static bool CanHaveMoreWagons(BasePlayer player)
        {
            if (!ownIns._config.AmountWagons)
            {
                return true;
            }

            int maxAmountWagons = 0;
            foreach (KeyValuePair<string, int> kvp in ownIns._config.PermissionsForAmountWagons)
            {
                if (
                    ownIns.permission.UserHasPermission(player.UserIDString, kvp.Key)
                    && kvp.Value > maxAmountWagons
                )
                {
                    maxAmountWagons = kvp.Value;
                }
            }

            if (
                (maxAmountWagons == 0)
                || (
                    ownIns.amountWagons.ContainsKey(player)
                    && ownIns.amountWagons[player] >= maxAmountWagons
                )
            )
            {
                NotifyManager.SendMessageToPlayer(
                    player,
                    "NotHavePermForMoreWagons",
                    ownIns._config.NotifyConfig.prefix
                );
                return false;
            }

            return true;
        }

        private void TryCreateNewGarage(
            BaseEntity baseEntity,
            BaseEntity targetEntity,
            BasePlayer player
        )
        {
            if (!IsExistCupboard(baseEntity, player))
            {
                return;
            }

            if (!IsTargetValid(baseEntity, targetEntity, player))
            {
                return;
            }

            baseEntity.Kill();
            TryCreateNewPosInGarage(targetEntity, player, player.GetBuildingPrivilege());
        }

        private bool IsExistCupboard(BaseEntity baseEntity, BasePlayer player)
        {
            BuildingPrivlidge cupboard = player.GetBuildingPrivilege();
            if (cupboard == null)
            {
                NotifyManager.SendMessageToPlayer(
                    player,
                    "BaseHasntCupboard",
                    ownIns._config.NotifyConfig.prefix
                );
                CancelAction(baseEntity, player);
                return false;
            }
            return true;
        }

        private static bool IsTargetValid(
            BaseEntity baseEntity,
            BaseEntity targetEntity,
            BasePlayer player
        )
        {
            if (
                targetEntity is not BuildingBlock
                || targetEntity.PrefabName
                    != "assets/prefabs/building core/foundation/foundation.prefab"
            )
            {
                NotifyManager.SendMessageToPlayer(
                    player,
                    "NotValidTarget",
                    ownIns._config.NotifyConfig.prefix
                );
                if (baseEntity.ShortPrefabName == "electric.flasherlight.deployed")
                {
                    CancelAction(baseEntity, player);
                }

                return false;
            }

            return true;
        }

        private void TryCreateNewPosInGarage(
            BaseEntity targetEntity,
            BasePlayer player,
            BuildingPrivlidge cupboard,
            Wagon? wagon = null
        )
        {
            List<BuildingBlock> buildingBlocks = Pool.Get<List<BuildingBlock>>();
            ConstructionMatrix matrix = new();

            CreateListWithFoundations(buildingBlocks, targetEntity);
            if (!IsRightAmountFoundations(buildingBlocks, player))
            {
                Pool.FreeUnmanaged(ref buildingBlocks);
                return;
            }
            if (!IsValidConstruction(matrix, targetEntity, buildingBlocks, player, wagon))
            {
                Pool.FreeUnmanaged(ref buildingBlocks);
                return;
            }

            if (!ownIns.dicGarages.TryGetValue(cupboard, out Garage garage))
            {
                garage = new Garage();
                dicGarages.Add(cupboard, garage);
            }

            if (wagon == null)
            {
                wagon = GetNewWagon(player, matrix, garage);
            }
            else
            {
                wagon.MoveWagonInBase(matrix.posForSpawnWagon, garage);
            }

            garage.AddNewPos(matrix.posForSpawnWagon, matrix.blocksUnderWagon);
            garage.wagons.Add(matrix.posForSpawnWagon.Key, wagon);

            Pool.FreeUnmanaged(ref buildingBlocks);
        }

        private void CreateListWithFoundations(
            List<BuildingBlock> buildingBlocks,
            BaseEntity targetEntity
        )
        {
            Vis.Entities(targetEntity.transform.position, 11f, buildingBlocks);

            foreach (BuildingBlock buildingBlock in buildingBlocks.ToHashSet())
            {
                if (
                    buildingBlock.PrefabName
                    != "assets/prefabs/building core/foundation/foundation.prefab"
                )
                {
                    _ = buildingBlocks.Remove(buildingBlock);
                }
            }
        }

        private bool IsRightAmountFoundations(List<BuildingBlock> buildingBlocks, BasePlayer player)
        {
            if (buildingBlocks.Count < 21)
            {
                NotifyManager.SendMessageToPlayer(
                    player,
                    "NoNeedAmountBlocks",
                    ownIns._config.NotifyConfig.prefix
                );
                GiveWagon(player, 1);
                return false;
            }
            return true;
        }

        private bool IsValidConstruction(
            ConstructionMatrix matrix,
            BaseEntity baseEntity,
            List<BuildingBlock> buildingBlocks,
            BasePlayer player,
            Wagon wagon
        )
        {
            matrix.Init(baseEntity, buildingBlocks, player);
            if (!matrix.isValid)
            {
                NotifyManager.SendMessageToPlayer(
                    player,
                    "NotValidConstruction",
                    ownIns._config.NotifyConfig.prefix
                );
                if (wagon == null)
                {
                    GiveWagon(player, 1);
                }

                return false;
            }
            return true;
        }

        private static Wagon GetNewWagon(
            BasePlayer player,
            ConstructionMatrix matrix,
            Garage garage
        )
        {
            if (!ownIns.amountWagons.TryGetValue(player, out int value))
            {
                value = 0;
                ownIns.amountWagons.Add(player, value);
            }

            ownIns.amountWagons[player] = ++value;
            Wagon wagon = new();
            wagon.Init(player, matrix.posForSpawnWagon, garage);
            return wagon;
        }

        private bool IsTargetEntityBelongWagon(BaseEntity targetEntity, out Wagon wagon)
        {
            return allBaseEntityByWagons.TryGetValue(targetEntity.net.ID.Value, out wagon);
        }

        private void CheckWagonOnCustomMonument(
            BasePlayer player,
            BaseEntity baseEntity,
            BaseEntity targetEntity,
            Wagon wagon
        )
        {
            if (
                playersOnMonuments.TryGetValue(player, out CustomMonument monument1)
                && !monument1.IsWagonOnMonument(wagon.trainCar)
            )
            {
                return;
            }

            if (monument1 == null)
            {
                NotifyManager.SendMessageToPlayer(
                    player,
                    "NotOnBaseAndNotOnMonument",
                    ownIns._config.NotifyConfig.prefix
                );
                CancelAction(baseEntity, player);
            }
            else if (monument1.isBuildingState)
            {
                wagon.CheckNewEntity(baseEntity, targetEntity, player);
            }
            else
            {
                NotifyManager.SendMessageToPlayer(
                    player,
                    "ModeIsNotBuilding",
                    ownIns._config.NotifyConfig.prefix
                );
                CancelAction(baseEntity, player);
            }
        }

        private void DetermineAllMonumentsForWagon()
        {
            if (customMonuments.Count > 0)
            {
                return;
            }

            foreach (BaseNetworkable entity in BaseNetworkable.serverEntities)
            {
                ElectricalBranch branch = entity as ElectricalBranch;
                if (branch == null || branch.branchAmount != 888444)
                {
                    continue;
                }

                CustomMonument monument = new GameObject().AddComponent<CustomMonument>();
                monument.Init(branch);
                _ = customMonuments.Add(monument);
            }
        }

        private void DestroyStandsAndMods()
        {
            foreach (Stand stand in allStands.Values.ToHashSet())
            {
                stand.Destroy();
            }

            foreach (Wagon wagon in wagons.ToHashSet())
            {
                wagon.DestroyModification();
            }
        }

        private void DestroyAllCustomMonuments()
        {
            foreach (CustomMonument monument in customMonuments)
            {
                monument.Destroy();
                UnityEngine.Object.Destroy(monument.gameObject);
            }
        }

        private void DetermineSettingsEntities()
        {
            foreach (AcceptableEntities entity in ownIns._config.AcceptableEntities)
            {
                acceptableEntities.Add(
                    entity.ShortName,
                    new SettingsEntity { IsAllowPut = entity.IsAllowPut, Amount = entity.Amount }
                );
            }
        }

        private static void CopySerializableFields<T>(T src, T dst)
        {
            // ВНИМАНИЕ: reflection запрещён. Необходимо реализовать ручное копирование для используемых типов.
            // Пример для DoorCloser -> ModificationsWagon и TrainCar -> BaseCombatEntity
            // Здесь оставляем пустым или реализуем для конкретных случаев ниже.
        }

        private static Quaternion GetGlobalRot(Transform Transform, Vector3 localRotation)
        {
            return Transform.rotation * Quaternion.Euler(localRotation);
        }

        private static Vector3 GetGlobalPos(Transform Transform, Vector3 localPosition)
        {
            return Transform.TransformPoint(localPosition);
        }

        private static void GiveWagon(BasePlayer player, int amount)
        {
            Item item = ItemManager.CreateByName("electric.flasherlight", amount, SkindIdFlasher);
            item.name = "Wagon";
            MoveItem(player, item);
        }

        private static void CancelAction(BaseEntity entity, BasePlayer player)
        {
            if (entity is BuildingBlock block)
            {
                ReturnResourcesToPlayer(block, player);
            }
            else
            {
                ReturnEntityToPlayer(entity, player);
            }

            if (entity.IsExists())
            {
                entity.Kill();
            }
        }

        private static void ReturnResourcesToPlayer(BuildingBlock buildingBlock, BasePlayer player)
        {
            foreach (ItemAmount itemAmount in buildingBlock.BuildCost())
            {
                Item item = ItemManager.CreateByItemID(itemAmount.itemid, (int)itemAmount.amount);
                MoveItem(player, item);
            }
        }

        private static void ReturnEntityToPlayer(BaseEntity entity, BasePlayer player)
        {
            BaseCombatEntity baseCombatEntity = entity as BaseCombatEntity;
            string strName = GetItemName(baseCombatEntity);
            Item item = ItemManager.CreateByName(strName, 1, entity.skinID);
            MoveItem(player, item);
        }

        private static string GetItemName(BaseCombatEntity baseCombatEntity)
        {
            if (baseCombatEntity.pickup.itemTarget == null)
            {
                return ownIns.entityToItem.TryGetValue(
                    baseCombatEntity.PrefabName,
                    out string strName
                )
                    ? strName
                    : GetShortNameItem(baseCombatEntity);
            }
            return baseCombatEntity.pickup.itemTarget.shortname;
        }

        private static string? GetShortNameItem(BaseCombatEntity baseCombatEntity)
        {
            foreach (ItemDefinition itemDef in ItemManager.GetItemDefinitions())
            {
                if (itemDef == null)
                {
                    continue;
                }

                ItemModDeployable itemMod = itemDef.GetComponent<ItemModDeployable>();
                if (itemMod == null || itemMod.entityPrefab == null)
                {
                    continue;
                }

                string path = itemMod.entityPrefab.resourcePath;
                if (ownIns.entityToItem.ContainsKey(path))
                {
                    continue;
                }

                if (path == baseCombatEntity.PrefabName)
                {
                    ownIns.entityToItem.Add(path, itemDef.shortname);
                    return itemDef.shortname;
                }
            }

            return null;
        }

        private static bool IsValidVariablesForBase(
            BasePlayer player,
            BaseEntity entity,
            out BuildingPrivlidge cupboard
        )
        {
            cupboard = null;

            if (!player.IsPlayer())
            {
                return false;
            }

            if (entity == null)
            {
                return false;
            }

            cupboard = player.GetBuildingPrivilege();
            if (cupboard == null)
            {
                NotifyManager.SendMessageToPlayer(
                    player,
                    "BaseHasntCupboard",
                    ownIns._config.NotifyConfig.prefix
                );
                return false;
            }
            if (cupboard?.IsAuthed(player) == false)
            {
                NotifyManager.SendMessageToPlayer(
                    player,
                    "NotAuthedInCupboard",
                    ownIns._config.NotifyConfig.prefix
                );
                return false;
            }

            return true;
        }

        private static HashSet<T> GetEntities<T>(Vector3 position, float radius, int layerMask = -1)
            where T : BaseEntity
        {
            HashSet<T> result = new();
            List<T> list = Pool.Get<List<T>>();
            Vis.Entities(position, radius, list, layerMask);
            foreach (T entity in list)
            {
                _ = result.Add(entity);
            }

            Pool.FreeUnmanaged(ref list);
            return result;
        }

        public static void Debug(object format)
        {
            ownIns.Puts(format.ToString());
        }

        public static void DebugError(object format)
        {
            ownIns.PrintError(format.ToString());
        }

        #endregion Helpers

        #region Variables

        [PluginReference]
        private readonly Plugin? GUIAnnouncements,
            DiscordMessages,
using System.Collections;
using Facepunch;
using Network;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using Oxide.Plugins.TrainHomesExtensionMethods;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable HeuristicUnreachableCode

namespace Oxide.Plugins
    {
        [Info("TrainHomes", "jtedal", "1.1.5")]
        internal sealed class TrainHomes : RustPlugin
        {
            private const bool En = false;

            #region Config

            internal required PluginConfig _config;

            public TrainHomes()
            {
                _config = PluginConfig.DefaultConfig();
            }

            protected override void LoadDefaultConfig()
            {
                Puts("Creating a default config...");
                _config = PluginConfig.DefaultConfig();
                _config.PluginVersion = Version;
                SaveConfig();
                Puts("Creation of the default config completed!");
            }

            protected override void LoadConfig()
            {
                base.LoadConfig();
                _config = Config.ReadObject<PluginConfig>();
                if (_config.PluginVersion < Version)
                {
                    UpdateConfigValues();
                }
            }

            private void UpdateConfigValues()
            {
                Puts("Config update detected! Updating config values...");
                if (_config.PluginVersion < new VersionNumber(1, 0, 1))
                {
                    _config.PermissionsForAmountWagons = new Dictionary<string, int>()
                    {
                        ["trainhomes.amount1"] = 1,
                        ["trainhomes.amount10"] = 10,
                    };
                }
                if (_config.PluginVersion < new VersionNumber(1, 0, 5))
                {
                    _config.CheckTopology = false;
                }
                if (_config.PluginVersion < new VersionNumber(1, 0, 7))
                {
                    _config.Log = true;
                }
                if (_config.PluginVersion < new VersionNumber(1, 1, 3))
                {
                    _config.IsAllowEditWagonOnBase = true;
                }
                _config.PluginVersion = Version;
                Puts("Config update completed!");
                SaveConfig();
            }

            protected override void SaveConfig()
            {
                Config.WriteObject(_config);
            }

            public sealed class NotifyConfig
            {
                [JsonProperty(En ? "Prefix" : "Префикс сообщений")]
                public string? prefix { get; set; }

                [JsonProperty(En ? "Chat Message setting" : "Настройки сообщений в чате")]
                public ChatConfig? chatConfig { get; set; }

                [JsonProperty(
                    En ? "Facepunch Game Tips setting" : "Настройка сообщений Facepunch Game Tip"
                )]
                public GameTipConfig? gameTipConfig { get; set; }

                [JsonProperty(
                    En
                        ? "GUI Announcements setting (only for GUIAnnouncements plugin)"
                        : "Настройка GUI Announcements (только для тех, кто использует плагин GUI Announcements)"
                )]
                public GuiAnnouncementsConfig? guiAnnouncementsConfig { get; set; }

                [JsonProperty(
                    En
                        ? "Notify setting (only for Notify plugin)"
                        : "Настройка Notify (только для тех, кто использует плагин Notify)"
                )]
                public NotifyPluginConfig? notifyPluginConfig { get; set; }

                [JsonProperty(
                    En
                        ? "Discord setting (only for DiscordMessages plugin)"
                        : "Настройка оповещений в Discord (только для тех, кто использует плагин Discord Messages)"
                )]
                public DiscordMessagesConfig? discordMessagesConfig { get; set; }
            }

            public sealed class ChatConfig
            {
                [JsonProperty(
                    En ? "Use chat notifications? [true/false]" : "Использовать ли чат? [true/false]"
                )]
                public bool isEnabled { get; set; }
            }

            public sealed class GameTipConfig
            {
                [JsonProperty(
                    En
                        ? "Use Facepunch Game Tips (notification bar above hotbar)? [true/false]"
                        : "Использовать ли Facepunch Game Tip (оповещения над слотами быстрого доступа игрока)? [true/false]"
                )]
                public bool isEnabled { get; set; }

                [JsonProperty(
                    En
                        ? "Style (0 - Blue Normal, 1 - Red Normal, 2 - Blue Long, 3 - Blue Short, 4 - Server Event)"
                        : "Стиль (0 - Blue Normal, 1 - Red Normal, 2 - Blue Long, 3 - Blue Short, 4 - Server Event)"
                )]
                public int style { get; set; }
            }

            public sealed class GuiAnnouncementsConfig
            {
                [JsonProperty(
                    En
                        ? "Do you use GUI Announcements integration? [true/false]"
                        : "Использовать ли GUI Announcements? [true/false]"
                )]
                public bool isEnabled { get; set; }

                [JsonProperty(En ? "Banner color" : "Цвет баннера")]
                public string? bannerColor { get; set; }

                [JsonProperty(En ? "Text color" : "Цвет текста")]
                public string? textColor { get; set; }

                [JsonProperty(En ? "Adjust Vertical Position" : "Отступ от верхнего края")]
                public float apiAdjustVPosition { get; set; }
            }

            public sealed class NotifyPluginConfig
            {
                [JsonProperty(
                    En
                        ? "Do you use Notify integration? [true/false]"
                        : "Использовать ли Notify? [true/false]"
                )]
                public bool isEnabled { get; set; }

                [JsonProperty(En ? "Type" : "Тип")]
                public string? type { get; set; }
            }

            public sealed class DiscordMessagesConfig
            {
                [JsonProperty(
                    En
                        ? "Do you use DiscordMessages? [true/false]"
                        : "Использовать ли DiscordMessages? [true/false]"
                )]
                public bool isEnabled { get; set; }

                [JsonProperty("Webhook URL")]
                public string? webhookUrl { get; set; }

                [JsonProperty(En ? "Embed Color (DECIMAL)" : "Цвет полосы (DECIMAL)")]
                public int embedColor { get; set; }

                [JsonProperty(En ? "Keys of required messages" : "Ключи необходимых сообщений")]
                public HashSet<string>? keys { get; set; }
            }

            public sealed class AcceptableEntities
            {
                [JsonProperty(nameof(ShortName))]
                public string? ShortName { get; set; }

                [JsonProperty(En ? "Allow this entity to be placed" : "Разрешить ставить этот предмет")]
                public bool IsAllowPut { get; set; }

                [JsonProperty(
                    En
                        ? "The maximum amount that can be placed in one wagon (If previous parameter true)"
                        : "Максимальное количество, которое можно ставить в одном вагоне (Если стоит true)"
                )]
                public int Amount { get; set; }
            }

            public sealed class PermissionsConfig
            {
                [JsonProperty(
                    En
                        ? "Permission name (Example: trainhomes.name)"
                        : "Название пермишена (Пример: trainhomes.name)"
                )]
                public string? Name { get; set; }
            }

            public sealed class CrateConfig
            {
                [JsonProperty(nameof(Prefab))]
                public string? Prefab { get; set; }

                [JsonProperty(En ? "Chance [0.0-100.0]" : "Шанс выпадения предмета [0.0-100.0]")]
                public float Chance { get; set; }
            }

            public sealed class PluginConfig
            {
                [JsonProperty(En ? "Record plugin logs" : "Записывать логи плагина")]
                public bool Log { get; set; }

                [JsonProperty(
                    En
                        ? "Allow to place and edit wagon on foundations"
                        : "Разрешить ставить и редактировать вагон на фундаментах"
                )]
                public bool IsAllowEditWagonOnBase { get; set; }

                [JsonProperty(
                    En
                        ? "Allow items to be placed on the roof of the wagon"
                        : "Разрешить ставить предметы на крыше поезда"
                )]
                public bool IsAllowPutOnRoof { get; set; }

                [JsonProperty(
                    En
                        ? "Enable topology checking when a player calls a wagon onto the tracks"
                        : "Включить проверку топологий, когда игрок вызывает вагон на рельсы"
                )]
                public bool CheckTopology { get; set; }

                [JsonProperty(
                    En
                        ? "The maximum distance at which a wagon can be moved"
                        : "Максимальная дистанция при которой вагон может быть перемещен"
                )]
                public int MaxDistance { get; set; }

                [JsonProperty(En ? "HP of the wagon" : "HP вагона")]
                public int HpWagon { get; set; }

                [JsonProperty(En ? "HP of the wagon stand" : "HP подставки")]
                public int HpBarricade { get; set; }

                [JsonProperty(
                    En
                        ? "A constant amount of free wagons on the tracks"
                        : "Постоянное количество бесплатных вагонов на рельсах"
                )]
                public int amountFreeWagons { get; set; }

                [JsonProperty(
                    En ? "Wagon spawn settings in crates" : "Настройка появления вагона в ящиках"
                )]
                public HashSet<CrateConfig>? Crates { get; set; }

                [JsonProperty(
                    En
                        ? "Whether to use permits for the number of wagons"
                        : "Использовать ли пермишены на кол-во вагонов"
                )]
                public bool AmountWagons { get; set; }

                [JsonProperty(
                    En
                        ? "Permissions for amount wagons in using"
                        : "Пермишены на кол-во вагонов для игрока"
                )]
                public Dictionary<string, int>? PermissionsForAmountWagons { get; set; }

                [JsonProperty(En ? "List of entities" : "Список Entity")]
                public HashSet<AcceptableEntities>? AcceptableEntities { get; set; }

                [JsonProperty(En ? "Notification Settings" : "Настройки уведомлений")]
                public NotifyConfig? NotifyConfig { get; set; }

                [JsonProperty(En ? "Configuration Version" : "Версия конфигурации")]
                public VersionNumber PluginVersion { get; set; }

                public static PluginConfig DefaultConfig()
                {
                    return new PluginConfig()
                    {
                        Log = true,
                        IsAllowEditWagonOnBase = true,
                        IsAllowPutOnRoof = true,
                        CheckTopology = false,
                        MaxDistance = 1500,
                        HpWagon = 5000,
                        HpBarricade = 1000,
                        amountFreeWagons = 5,
                        Crates = new HashSet<CrateConfig>()
                    {
                        new()
                        {
                            Prefab = "assets/bundled/prefabs/radtown/crate_normal.prefab",
                            Chance = 100f,
                        },
                        new()
                        {
                            Prefab = "assets/bundled/prefabs/radtown/crate_elite.prefab",
                            Chance = 50f,
                        },
                    },
                        AmountWagons = false,
                        PermissionsForAmountWagons = new Dictionary<string, int>()
                        {
                            ["trainhomes.amount1"] = 1,
                            ["trainhomes.amount10"] = 10,
                        },
                        AcceptableEntities = new HashSet<AcceptableEntities>()
                    {
                        new()
                        {
                            ShortName = "box.wooden.large",
                            IsAllowPut = true,
                            Amount = 100,
                        },
                    },
                        NotifyConfig = new NotifyConfig
                        {
                            prefix = "[TrainHomes]",
                            chatConfig = new ChatConfig { isEnabled = true },
                            gameTipConfig = new GameTipConfig { isEnabled = false, style = 2 },
                            guiAnnouncementsConfig = new GuiAnnouncementsConfig
                            {
                                isEnabled = false,
                                bannerColor = "Grey",
                                textColor = "White",
                                apiAdjustVPosition = 0.03f,
                            },
                            notifyPluginConfig = new NotifyPluginConfig
                            {
                                isEnabled = false,
                                type = "0",
                            },
                            discordMessagesConfig = new DiscordMessagesConfig
                            {
                                isEnabled = false,
                                webhookUrl =
                                    "https://support.discordapp.com/hc/en-us/articles/228383668-Intro-to-Webhooks",
                                embedColor = 13516583,
                                keys = new HashSet<string> { "PreStart", "EventStart", "Finish" },
                            },
                        },
                        PluginVersion = new VersionNumber(),
                    };
                }
            }

            #endregion Config

            #region Lang

            protected override void LoadDefaultMessages()
            {
                lang.RegisterMessages(
                    new Dictionary<string, string>
                    {
                        ["BaseHasntCupboard"] = "{0} You can't do this because there is no cupboard.",
                        ["NotAuthedInCupboard"] =
                            "{0} You can't do this because you're not authorized in the cupboard.",
                        ["NoNeedAmountBlocks"] =
                            "{0} Your base must have at least 21 square foundations. (Details: /thinstruction)",
                        ["NotValidPosForTrainCar"] = "{0} TrainCar cannot be spawned in this position.",
                        ["CanntPutEntityOnRoof"] = "{0} You can't put objects on the roof.",
                        ["CanntPutEntity"] = "{0} You can't put this object on the wagon.",
                        ["SetMaxAmountObjects"] =
                            "{0} You have set the maximum amount of these objects on the wagon.",
                        ["LimitFloorsForHouse"] =
                            "{0} Building blocks cannot be outside the wagon or above the 1st floor.",
                        ["CodeOfTheWagon"] =
                            "{0} Your code from the wagon [{1}]. You can change these parameters.",
                        ["FrequencyOfTheWagon"] =
                            "{0} Your frequency from the wagon [{1}]. You can change this parameter.",
                        ["LongDistance"] =
                            "{0} The distance from the wagon to the spawn point is too long. (Distance: {1}m, Maximum distance: {2}m)",
                        ["TwoCallers"] =
                            "{0} When calling a wagon, the detonator must be in the hand of only 1 player.",
                        ["WagonAlreadyInGarage"] = "{0} The wagon is already in another base.",
                        ["WagonAlreadyOnTracks"] = "{0} The wagon is already on tracks.",
                        ["NotValidTopologies"] =
                            "{0} There are no topologies for spawn in this position. Try to spawn the wagon in another position or contact the server owner.",
                        ["TimerForSpawn"] = "{0} The wagon will spawn in {1}...",
                        ["NotValidConstruction"] =
                            "{0} Incorrect сonstruction for the wagon spawn. (Details: /thinstruction)",
                        ["NotValidTarget"] =
                            "{0} Incorrect сonstruction for the wagon spawn or cannot be spawned in this position. (Подробности: /thinstruction)",
                        ["FoundationsExistInHashSet"] =
                            "{0} You are trying to spawn the wagon in a place where there is already a position for the wagon. (Подробности: /thinstruction)",
                        ["CommandForAdminsOnly"] = "{0} This command is for admins only.",
                        ["RemoveTimeEnd"] = "{0} The time for removing the wagon is over.",
                        ["CantCallAnotherWagon"] = "{0} Wait until the other wagon is spawned.",
                        ["WagonAlreadyCalled"] =
                            "{0} This wagon has already been called by another player.",
                        ["WagonOnMonument"] =
                            "{0} This wagon is located on the custom monument, in building mode. You can't move it.",
                        ["ModeIsNotBuilding"] =
                            "{0} Turn on the monument building mode using the button on the pillar.",
                        ["NotOnBaseAndNotOnMonument"] =
                            "{0} You can't build on a wagon not on base and not on a special custom monument.",
                        ["MuchWagons"] =
                            "{0} To enable building mode, there should not be more than 1 wagon inside the monument.",
                        ["NotCustomWagon"] =
                            "{0} To enable building mode, there should be a custom wagon inside the monument.",
                        ["NotHaveWagons"] =
                            "{0} To enable building mode, there should be a wagon inside the monument.",
                        ["InvalidCmdChat1"] =
                            "{0} You <color=#ce3f27>didn't</color> write amount and player's SteamID! \n/givewagon <amount> <SteamID>",
                        ["InvalidCmdChat2"] =
                            "{0} You <color=#ce3f27>didn't</color> write some player's SteamID! \n/givewagon <amount> <SteamID>",
                        ["InvalidCmdChat3"] =
                            "{0} Player with SteamID: {1} <color=#ce3f27>not found</color>!",
                        ["ValidCmdChat"] = "{0} The wagon was successfully given to the player!",
                        ["YouNotOwner"] =
                            "{0} You can't do this because you are not the owner of the wagon!",
                        ["CantBuildingOnFreeWagon"] =
                            "{0} You can't build on the free wagon! Set the code for the codelock and try again.",
                        ["NotHavePerm"] = "{0} You dont have permission to use this command.",
                        ["NotHavePermForMoreWagons"] =
                            "{0} You do not have perms to own a larger number of wagons.",
                        ["NotValidRaycast"] =
                            "{0} The object has not been found or it does not meet the conditions for recovery the wagon.",

                        ["GUI_General"] = "General",
                        ["GUI_General_Description"] =
                            "1) In the crates, you can find an item for the wagon spawn."
                            + "\n2) On the railway tracks, occasionally, you can find an free wagon. To become its owner, enter any code."
                            + "\n3) You can only place the wagon at your base. For details, see the section 'Construction'."
                            + "\n4) You can build on a wagon only at your base or on a custom monument, if there is one. For more information, see the section 'Custom monument'."
                            + "\n5) Any player can build on the wagon."
                            + "\n6) There is no need to install a cupboard on the wagon, because the objects on it do not decay, as well as the wagon itself."
                            + "\n7) To move the wagon from the base to the rails and back, you will need a detonator. Enter the frequency that is indicated on the wagon, stand in front of the place where you want to move it and press the button."
                            + "\n8) All players who are authorized in the codelock have access to the wagon movement."
                            + "\n9) Any player can change the frequency of the wagon."
                            + "\n10) The owner of the wagon can remove his wagon and get a spawn item in return. Be careful! All objects on it will destroy, and resources will not be returned. (Command: /removewagon)",

                        ["GUI_Construction"] = "Construction",
                        ["GUI_Construction_Description"] =
                            "1) The сonstruction must contain 7x3 foundations."
                            + "\n2) To spawn the wagon, you must definitely put a cupboard."
                            + "\n3) All foundations must be at the same height."
                            + "\n4) If a cupboard, or one of the 21 foundations, or a stand breaks down on the сonstruction while the wagon is located, then the one will also be destroyed."
                            + "\n5) To place a wagon, you must strictly specify the foundation that is the most central of the 21."
                            + "\n6) The wagon must not be placed on foundations other than those.",

                        ["GUI_CustomMonument"] = "Custom Monument",
                        ["GUI_CustomMonument_Description"] =
                            "1) You will not be able to build anything on the wagon until you activate the building mode."
                            + "\n2) The construction mode is activated by pressing a button on one of the pillars."
                            + "\n3) After pressing the button, the wagon will automatically reach the desired position if it is not entirely located in the construction area."
                            + "\n4) Be careful! During the building mode, any player will be able to build anything on your wagon."
                            + "\n5) To exit the building mode, press the button again.",
                    },
                    this
                );

                lang.RegisterMessages(
                    new Dictionary<string, string>
                    {
                        ["BaseHasntCupboard"] = "{0} Вы не можете это сделать, потому что нет шкафа.",
                        ["NotAuthedInCupboard"] =
                            "{0} Вы не можете это сделать, потому что не авторизованы в шкафу.",
                        ["NoNeedAmountBlocks"] =
                            "{0} У вашей постройки должно быть минимум 21 квадратных фундаментов. (Подробности: /thinstruction)",
                        ["NotValidPosForTrainCar"] = "{0} В этой позиции нельзя заспавнить вагон.",
                        ["CanntPutEntityOnRoof"] = "{0} Нельзя ставить объекты на крыше.",
                        ["CanntPutEntity"] = "{0} Нельзя ставить этот объект на поезде.",
                        ["SetMaxAmountObjects"] =
                            "{0} Вы установили максимальное количество этих объектов на вагоне.",
                        ["LimitFloorsForHouse"] =
                            "{0} Строительные блоки не могут выходить за пределы одноэтажной конструкции.",
                        ["CodeOfTheWagon"] =
                            "{0} Ваш код от вагона [{1}]. Вы можете изменить этот параметр.",
                        ["FrequencyOfTheWagon"] =
                            "{0} Ваша частота от вагона [{1}]. Вы можете изменить этот параметр.",
                        ["LongDistance"] =
                            "{0} Дистанция от вагона до точки спавна слишком большая. (Дистанция: {1}м, Максимальная дистанция: {2}м)",
                        ["TwoCallers"] =
                            "{0} При вызове вагона, детонатор должен быть в руке только 1 игрока.",
                        ["WagonAlreadyInGarage"] = "{0} Вагон уже находится на другой базе.",
                        ["WagonAlreadyOnTracks"] = "{0} Вагон уже находится на рельсах.",
                        ["NotValidTopologies"] =
                            "{0} В этой позиции нет топологий для спавна. Попробуйте заспавнить вагон в другом месте или обратитесь к владельцу сервера.",
                        ["TimerForSpawn"] = "{0} Вагон появится через {1}...",
                        ["NotValidConstruction"] =
                            "{0} Неправильная конструкция для спавна вагона. (Подробности: /thinstruction)",
                        ["NotValidTarget"] =
                            "{0} Неправильная конструкция или позиция для спавна вагона. (Подробности: /thinstruction)",
                        ["FoundationsExistInHashSet"] =
                            "{0} Вы пытаетесь заспавнить вагон в том месте, где уже есть позиция для вагона. (Подробности: /thinstruction)",
                        ["CommandForAdminsOnly"] = "{0} Эта команда только для админов.",
                        ["RemoveTimeEnd"] = "{0} Время для удаления вагона истекло.",
                        ["CantCallAnotherWagon"] = "{0} Дождитесь пока другой вагон будет заспавнен.",
                        ["WagonAlreadyCalled"] = "{0} Этот вагон уже вызвал другой игрок.",
                        ["WagonOnMonument"] =
                            "{0} Этот вагон находится на монументе, в режиме строитедства. Вы не можете его перемещать.",
                        ["ModeIsNotBuilding"] =
                            "{0} Включите режим строителтсва на монументе через кнопку на столбе.",
                        ["NotOnBaseAndNotOnMonument"] =
                            "{0} Вы не можете строить на вагоне не дома и не на специальном кастомном монументе.",
                        ["MuchWagons"] =
                            "{0} Чтобы включить режим редактирования, внутри монумента не должно быть более 1 вагона.",
                        ["NotCustomWagon"] =
                            "{0} Чтобы включить режим редактирования, внутри монумента должен быть вагон для строительста.",
                        ["NotHaveWagons"] =
                            "{0} Чтобы включить режим редактирования, внутри монумента должен быть вагон.",
                        ["InvalidCmdChat1"] =
                            "{0} Вы <color=#ce3f27>не написали</color> количество и SteamID игрока! \n/givewagon <amount> <SteamID>",
                        ["InvalidCmdChat2"] =
                            "{0} Вы <color=#ce3f27>не написали</color> SteamID игрока! \n/givewagon <amount> <SteamID>",
                        ["InvalidCmdChat3"] =
                            "{0} Игрок с SteamID: {1} <color=#ce3f27>не найден</color>",
                        ["ValidCmdChat"] = "{0} Вагон был удачно выдан игоку!",
                        ["YouNotOwner"] =
                            "{0} Вы не можете это сделать потому что не являетесь владельцем вагона!",
                        ["CantBuildingOnFreeWagon"] =
                            "{0} Строиться на свободном вагоне нельзя! Введите код для кодлока и повторите попытку.",
                        ["NotHavePerm"] = "{0} У вас нет разрешений на использование этой команды.",
                        ["NotHavePermForMoreWagons"] =
                            "{0} У вас нет разрешений на владение большего кол-ва вагонов.",
                        ["NotValidRaycast"] =
                            "{0} Объект не найден или он не соответствует условиям для восстановления вагона.",

                        ["GUI_General"] = "Общее",
                        ["GUI_General_Description"] =
                            "1) В ящиках, можно найти предмет для спавна вагона."
                            + "\n2) На железнодорожных путях, изредка, можно встретить свободный вагон. Чтобы стать его владельцем, введите любой код."
                            + "\n3) Разместить вагон вы можете только у себя на базе. Подробности смотрите в разделе 'Конструкция'."
                            + "\n4) Строиться на вагоне вы можете только у себя на базе или на кастомном монументе, если он есть. Подробности смотрите в разделе ‘Кастом монумент'."
                            + "\n5) Строиться на вагоне может любой игрок."
                            + "\n6) На вагоне не нужно устанавливать шкаф, поскольку объекты на нем не гниют, также как и сам вагон."
                            + "\n7) Для перемещения вагона с базы на рельсы и обратно, вам понадобиться детонатор. Введите частоту, которая указана на вагоне, встаньте перед тем место, куда хотите его переместить и нажмите кнопку."
                            + "\n8) Доступ к перемещению вагона имеют все игроки, которые авторизованы в кодлоке."
                            + "\n9) Частоту вагона может поменять любой игрок."
                            + "\n10) Владелец вагона может удалить свой вагон и взамен получить предмет для спавна. Будьте осторожны! Все объекты на нем пропадут, а ресурсы возвращены не будут. (Команда: /removewagon)",

                        ["GUI_Construction"] = "Конструкция",
                        ["GUI_Construction_Description"] =
                            "1) Конструкция должна содержать 7x3 фундамента."
                            + "\n2) Для размещения вагона вы обязательно должны поставить шкаф."
                            + "\n3) Все фундаменты должны быть на одной высоте."
                            + "\n4) Если во время нахождения вагона на конструкции сломается шкаф, или один из 21 фундамента, или подставка, то вагон также уничтожится."
                            + "\n5) Для размещения вагона, вы должны строго указывать тот фундамент, который самый центральный из 21."
                            + "\n6) Вагон нельзя размещать не на фундаментах.",

                        ["GUI_CustomMonument"] = "Кастом монумент",
                        ["GUI_CustomMonument_Description"] =
                            "1) Вы не сможете что либо построить на вагоне пока не активируете режим строительства."
                            + "\n2) Активация режима строительства происходит путем нажатия кнопки на одном из столбов."
                            + "\n3) После нажатия кнопки, вагон автоматически доедет до нужной позиции, если он целиком не находится в зоне строительства"
                            + "\n4) Будьте осторожны! Во время режима строительства, любой игрок сможет построить что либо на вашем вагоне."
                            + "\n5) Чтобы выйти из режима строительства, нажмите еще раз на кнопку.",
                    },
                    this,
                    "ru"
                );
            }

            private string GetMessage(string langKey, string userID)
            {
                return lang.GetMessage(langKey, this, userID);
            }

            private static string GetMessage(string langKey, string userID, params object[] args)
            {
                return (args.Length == 0)
                    ? GetMessage(langKey, userID)
                    : string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        GetMessage(langKey, userID),
                        args
                    );
            }

            #endregion Lang

            #region GUI

            [ConsoleCommand("Close_GUI")]
            private void DestroyGUI(ConsoleSystem.Arg arg)
            {
                GUIManager.DestroyGUI(arg.Player());
            }

            [ConsoleCommand("GUI_General")]
            private void CreateGUI_General(ConsoleSystem.Arg arg)
            {
                GUIManager.CreateGUI(arg.Player(), GUIManager.GUITopic.General);
            }

            [ConsoleCommand("GUI_Construction")]
            private void CreateGUI_Construction(ConsoleSystem.Arg arg)
            {
                GUIManager.CreateGUI(arg.Player(), GUIManager.GUITopic.Construction);
            }

            [ConsoleCommand("GUI_CustomMonument")]
            private void CreateGUI_CustomMonument(ConsoleSystem.Arg arg)
            {
                GUIManager.CreateGUI(arg.Player(), GUIManager.GUITopic.CustomMonument);
            }

            #endregion GUI

            #region Data System

            private void WriteData()
            {
                CreateData();
                Interface.Oxide.DataFileSystem.WriteObject("MM_Data/TrainHomes/Wagons", dataForWagons);
                Interface.Oxide.DataFileSystem.WriteObject(
                    "MM_Data/TrainHomes/Garages",
                    dataForGarages
                );
            }

            private void LoadFiles()
            {
                dataForWagons = Interface.Oxide.DataFileSystem.ReadObject<HashSet<WagonData>>(
                    "MM_Data/TrainHomes/Wagons"
                );
                dataForGarages = Interface.Oxide.DataFileSystem.ReadObject<HashSet<GarageData>>(
                    "MM_Data/TrainHomes/Garages"
                );

                LoadDataForWagon();
                LoadDataForGarage();
            }

            private void LoadDataForWagon()
            {
                foreach (WagonData wagonData in dataForWagons)
                {
                    Wagon wagon = new();
                    bool isValidData = true;
                    wagon.InitWithData(wagonData, ref isValidData);

                    if (isValidData)
                    {
                        _ = wagons.Add(wagon);
                    }
                    else
                    {
                        wagon.RemoveWagon();
                    }
                }
            }

            private void LoadDataForGarage()
            {
                foreach (GarageData garageData in dataForGarages)
                {
                    BuildingPrivlidge? cupboard =
                        BaseNetworkable.serverEntities.FirstOrDefault(
                            p => p.net.ID.Value == garageData.idCupboard
                        ) as BuildingPrivlidge;

                    Garage garage = new();
                    garage.InitWithData(garageData);

                    if (cupboard == null)
                    {
                        garage.DestroyAllWagons();
                        continue;
                    }

                    if (garage.wagons.Count > 0)
                    {
                        dicGarages.Add(cupboard, garage);
                    }
                }
            }

            private void CreateData()
            {
                dataForWagons.Clear();
                dataForGarages.Clear();
                CreateDataForWagon();
                CreateDataForGarage();
            }

            private void CreateDataForWagon()
            {
                foreach (Wagon wagon in wagons.ToHashSet())
                {
                    if (wagon?.freeWagon != false)
                    {
                        continue;
                    }

                    WagonData data = wagon.CreateInfoForData();
                    _ = dataForWagons.Add(data);
                }
            }

            private void CreateDataForGarage()
            {
                foreach (KeyValuePair<BuildingPrivlidge, Garage> garage in dicGarages)
                {
                    GarageData data = garage.Value.CreateInfoForData(garage.Key.net.ID.Value);
                    _ = dataForGarages.Add(data);
                }
            }

            #endregion Data System

            #region Oxide Hooks

            private void Init()
            {
                ownIns = this;
                PermissionManager.RegisterPermissions();
            }

            private void Unload()
            {
                UpdateWagonsForUnload();
                WriteData();
                DestroyStandsAndMods();
                DestroyAllCustomMonuments();
                dicGarages.Clear();
                ControllerSpawnsFreeWagon.Unload();
                ownIns = null;
            }

            private void OnServerInitialized()
            {
                if (GUIAnnouncements == null)
                {
                    PrintError("GUIAnnouncements plugin is not installed!");
                }

                if (DiscordMessages == null)
                {
                    PrintError("DiscordMessages plugin is not installed!");
                }

                if (Notify == null)
                {
                    PrintError("Notify plugin is not installed!");
                }

                if (MadMappersPluginUpdate == null)
                {
                    PrintError("MadMappersPluginUpdate plugin is not installed!");
                }

                if (ArmoredTrain == null)
                {
                    PrintError("ArmoredTrain plugin is not installed!");
                }

                PluginUpdateManager.CheckPluginUpdate();
                LoadFiles();

                DetermineSettingsEntities();

                ControllerSpawnsFreeWagon.Init();
                DetermineAllMonumentsForWagon();
            }

            private void OnServerSave()
            {
                WriteData();
            }

            [HookMethod("OnEntityKill")]
            private object? OnEntityKill(BaseEntity entity)
            {
                if (entity == null)
                {
                    return null;
                }

                _ = allBaseEntityByWagons.Remove(entity.net.ID.Value);
                if (allStands.TryGetValue(entity.net.ID.Value, out Stand? stand))
                {
                    OnRemoveWagon(stand, entity.GetComponent<BasePlayer>());
                }
                return null;
            }

            [HookMethod("OnWagonKill")]
            private object? OnWagonKill(BaseEntity entity)
            {
                if (entity == null)
                {
                    return null;
                }

                _ = allParents.Remove(entity.net.ID.Value);
                return null;
            }

            [HookMethod("OnTrainCarKill")]
            private object? OnTrainCarKill(BaseEntity entity)
            {
                if (entity == null)
                {
                    return null;
                }

                _ = allTrainCars.Remove(entity.net.ID.Value);
                return null;
            }

            [HookMethod("OnBuildingBlockKill")]
            private object? OnBuildingBlockKill(BaseEntity entity)
            {
                if (entity == null)
                {
                    return null;
                }

                _ = allFoundationsUnderWagons.Remove(entity.net.ID.Value);
                return null;
            }

            [HookMethod("OnCupboardKill")]
            private object? OnCupboardKill(BaseEntity entity)
            {
                if (entity == null)
                {
                    return null;
                }

                if (entity is BuildingPrivlidge cupboard)
                {
                    _ = dicGarages.Remove(cupboard);
                }

                return null;
            }

            private object? OnEntityTakeDamage(BaseCombatEntity baseCombatEntity, HitInfo info)
            {
                if (baseCombatEntity == null || info == null)
                {
                    return null;
                }

                if (baseCombatEntity.net == null)
                {
                    return null;
                }

                if (_config.Log)
                {
                    LogManager.AddLogDamage(baseCombatEntity, info);
                }

                return null;
            }

            private bool? OnEntityTakeDamage(BuildingBlock entity, HitInfo info)
            {
                if (entity == null || info == null)
                {
                    return null;
                }

                if (entity.net == null)
                {
                    return null;
                }

                if (
                    entity.PrefabName
                        == "assets/prefabs/building core/foundation.steps/foundation.steps.prefab"
                    && allObjectsOfWalls.Contains(entity.net.ID.Value)
                )
                {
                    return true;
                }

                return null;
            }

            private bool? OnEntityTakeDamage(LootContainer container, HitInfo info)
            {
                if (container == null || info == null)
                {
                    return null;
                }

                if (container.net == null)
                {
                    return null;
                }

                if (allObjectsOfWalls.Contains(container.net.ID.Value))
                {
                    return true;
                }

                return null;
            }

            private bool? OnEntityTakeDamage(Barricade barricade, HitInfo info)
            {
                if (barricade == null || barricade.net == null)
                {
                    return null;
                }

                if (allObjectsOfWalls.Contains(barricade.net.ID.Value))
                {
                    return true;
                }

                return null;
            }

            private bool? OnEntityTakeDamage(NeonSign neonSign, HitInfo info)
            {
                if (neonSign == null || neonSign.net == null)
                {
                    return null;
                }

                if (allObjectsOfWalls.Contains(neonSign.net.ID.Value))
                {
                    return true;
                }

                return null;
            }

            private bool? OnEntityTakeDamage(RFReceiver receiver, HitInfo info)
            {
                if (receiver == null)
                {
                    return null;
                }

                if (allReceivers.Contains(receiver))
                {
                    return true;
                }

                return null;
            }

            private bool? OnEntityTakeDamage(RANDSwitch randSwitch, HitInfo info)
            {
                if (randSwitch == null)
                {
                    return null;
                }

                if (allSwitches.Contains(randSwitch))
                {
                    return true;
                }

                return null;
            }

            private bool? OnStructureRepair(BaseEntity entity, BasePlayer player)
            {
                if (entity == null || entity.net == null)
                {
                    return null;
                }

                if (
                    playersWhoWantsToRemoveBaseWagon.Contains(player)
                    && allStands.TryGetValue(entity.net.ID.Value, out Stand stand)
                )
                {
                    OnRemoveWagon(stand, player);
                    return true;
                }

                return null;
            }

            private bool? CanPickupEntity(BasePlayer player, DoorCloser doorCloser)
            {
                if (doorCloser == null || doorCloser.net == null)
                {
                    return null;
                }

                if (doorCloser.skinID != SkindIdFlasher)
                {
                    return false;
                }

                return null;
            }

            private object? OnButtonPress(PressButton button, BasePlayer player)
            {
                if (button == null || !player.IsPlayer())
                {
                    return null;
                }

                if (buttonsOnMonuments.TryGetValue(button, out CustomMonument? monument))
                {
                    monument.TryUpdateState(player);
                }

                return null;
            }

            private void OnLootSpawn(LootContainer container)
            {
                if (container == null || container.inventory == null)
                {
                    return;
                }

                CrateConfig? config = _config.Crates.FirstOrDefault(
                    x => x.Prefab == container.PrefabName
                );
                if (config == null)
                {
                    return;
                }

                if (UnityEngine.Random.Range(0f, 100f) > config.Chance)
                {
                    return;
                }

                if (container.inventory.itemList.Count == container.inventory.capacity)
                {
                    container.inventory.capacity++;
                }

                Item wagon = ItemManager.CreateByName("electric.flasherlight", 1, SkindIdFlasher);
                wagon.name = "Wagon";
                if (!wagon.MoveToContainer(container.inventory))
                {
                    wagon.Remove();
                }
            }

            private bool? CanChangeCode(
                BasePlayer player,
                CodeLock codeLock,
                string newCode,
                bool isGuestCode
            )
            {
                if (allCodeLocks.Contains(codeLock))
                {
                    ModificationsWagon modifications = codeLock.GetParentEntity() as ModificationsWagon;
                    if (modifications != null)
                    {
                        if (!CanHaveMoreWagons(player))
                        {
                            return true;
                        }

                        if (!amountWagons.TryGetValue(player, out int value))
                        {
                            value = 0;
                            amountWagons.Add(player, value);
                        }

                        amountWagons[player] = ++value;
                        NextTick(() => modifications.ChangeStateToLocked(player));
                    }
                }
                return null;
            }

            private void OnEntitySpawned(TrainCar trainCar)
            {
                NextTick(() => ControllerSpawnsFreeWagon.TryAddNewTrainCar(trainCar));
            }

            #endregion Oxide Hooks

            #region RustEdit Hooks

            private void RustEdit_OnMapDataProcessed()
            {
                DetermineAllMonumentsForWagon();
            }

            #endregion RustEdit Hooks

            #region Helpers

            private static void UpdateWagonsForUnload()
            {
                if (ownIns == null)
                {
                    throw new InvalidOperationException("Plugin instance is not initialized");
                }
                foreach (Wagon wagon in ownIns.wagons)
                {
                    if (wagon.onCustomMonument)
                    {
                        wagon.UpdateForNotBuildingBlocks();
                    }
                }
            }

            private static void OnRemoveWagon(Stand stand, BasePlayer player)
            {
                GiveWagon(player, 1);

                if (stand.wagon.owner == player)
                {
                    stand.wagon.RemoveWagon();
                }
                else
                {
                    NotifyManager.SendMessageToPlayer(
                        player,
                        "YouNotOwner",
                        ownIns._config.NotifyConfig.prefix
                    );
                }
            }

            private void CheckBaseEntity(
                BaseEntity baseEntity,
                Construction construction,
                Construction.Target target,
                BasePlayer player
            )
            {
                if (!player.IsPlayer())
                {
                    return;
                }

                if (baseEntity == null)
                {
                    return;
                }

                if (IsItItemForWagonSpawn(baseEntity))
                {
                    if (!CanHaveMoreWagons(player) || !_config.IsAllowEditWagonOnBase)
                    {
                        CancelAction(baseEntity, player);
                        return;
                    }
                    TryCreateNewGarage(baseEntity, target.entity, player);
                }

                if (target.entity == null)
                {
                    return;
                }

                if (!IsTargetEntityBelongWagon(target.entity, out Wagon wagon))
                {
                    return;
                }

                if (wagon.parent != null)
                {
                    wagon.CheckNewEntity(baseEntity, target.entity, player);
                }
                else
                {
                    CheckWagonOnCustomMonument(player, baseEntity, target.entity, wagon);
                }
            }

            private static bool IsItItemForWagonSpawn(BaseEntity baseEntity)
            {
                return baseEntity.ShortPrefabName == "electric.flasherlight.deployed"
                    && baseEntity.skinID == SkindIdFlasher;
            }

            private static bool CanHaveMoreWagons(BasePlayer player)
            {
                if (!ownIns._config.AmountWagons)
                {
                    return true;
                }

                int maxAmountWagons = 0;
                foreach (KeyValuePair<string, int> kvp in ownIns._config.PermissionsForAmountWagons)
                {
                    if (
                        ownIns.permission.UserHasPermission(player.UserIDString, kvp.Key)
                        && kvp.Value > maxAmountWagons
                    )
                    {
                        maxAmountWagons = kvp.Value;
                    }
                }

                if (
                    (maxAmountWagons == 0)
                    || (
                        ownIns.amountWagons.ContainsKey(player)
                        && ownIns.amountWagons[player] >= maxAmountWagons
                    )
                )
                {
                    NotifyManager.SendMessageToPlayer(
                        player,
                        "NotHavePermForMoreWagons",
                        ownIns._config.NotifyConfig.prefix
                    );
                    return false;
                }

                return true;
            }

            private void TryCreateNewGarage(
                BaseEntity baseEntity,
                BaseEntity targetEntity,
                BasePlayer player
            )
            {
                if (!IsExistCupboard(baseEntity, player))
                {
                    return;
                }

                if (!IsTargetValid(baseEntity, targetEntity, player))
                {
                    return;
                }

                baseEntity.Kill();
                TryCreateNewPosInGarage(targetEntity, player, player.GetBuildingPrivilege());
            }

            private bool IsExistCupboard(BaseEntity baseEntity, BasePlayer player)
            {
                BuildingPrivlidge cupboard = player.GetBuildingPrivilege();
                if (cupboard == null)
                {
                    NotifyManager.SendMessageToPlayer(
                        player,
                        "BaseHasntCupboard",
                        ownIns._config.NotifyConfig.prefix
                    );
                    CancelAction(baseEntity, player);
                    return false;
                }
                return true;
            }

            private static bool IsTargetValid(
                BaseEntity baseEntity,
                BaseEntity targetEntity,
                BasePlayer player
            )
            {
                if (
                    targetEntity is not BuildingBlock
                    || targetEntity.PrefabName
                        != "assets/prefabs/building core/foundation/foundation.prefab"
                )
                {
                    NotifyManager.SendMessageToPlayer(
                        player,
                        "NotValidTarget",
                        ownIns._config.NotifyConfig.prefix
                    );
                    if (baseEntity.ShortPrefabName == "electric.flasherlight.deployed")
                    {
                        CancelAction(baseEntity, player);
                    }

                    return false;
                }

                return true;
            }

            private void TryCreateNewPosInGarage(
                BaseEntity targetEntity,
                BasePlayer player,
                BuildingPrivlidge cupboard,
                Wagon? wagon = null
            )
            {
                List<BuildingBlock> buildingBlocks = Pool.Get<List<BuildingBlock>>();
                ConstructionMatrix matrix = new();

                CreateListWithFoundations(buildingBlocks, targetEntity);
                if (!IsRightAmountFoundations(buildingBlocks, player))
                {
                    Pool.FreeUnmanaged(ref buildingBlocks);
                    return;
                }
                if (!IsValidConstruction(matrix, targetEntity, buildingBlocks, player, wagon))
                {
                    Pool.FreeUnmanaged(ref buildingBlocks);
                    return;
                }

                if (!ownIns.dicGarages.TryGetValue(cupboard, out Garage garage))
                {
                    garage = new Garage();
                    dicGarages.Add(cupboard, garage);
                }

                if (wagon == null)
                {
                    wagon = GetNewWagon(player, matrix, garage);
                }
                else
                {
                    wagon.MoveWagonInBase(matrix.posForSpawnWagon, garage);
                }

                garage.AddNewPos(matrix.posForSpawnWagon, matrix.blocksUnderWagon);
                garage.wagons.Add(matrix.posForSpawnWagon.Key, wagon);

                Pool.FreeUnmanaged(ref buildingBlocks);
            }

            private void CreateListWithFoundations(
                List<BuildingBlock> buildingBlocks,
                BaseEntity targetEntity
            )
            {
                Vis.Entities(targetEntity.transform.position, 11f, buildingBlocks);

                foreach (BuildingBlock buildingBlock in buildingBlocks.ToHashSet())
                {
                    if (
                        buildingBlock.PrefabName
                        != "assets/prefabs/building core/foundation/foundation.prefab"
                    )
                    {
                        _ = buildingBlocks.Remove(buildingBlock);
                    }
                }
            }

            private bool IsRightAmountFoundations(List<BuildingBlock> buildingBlocks, BasePlayer player)
            {
                if (buildingBlocks.Count < 21)
                {
                    NotifyManager.SendMessageToPlayer(
                        player,
                        "NoNeedAmountBlocks",
                        ownIns._config.NotifyConfig.prefix
                    );
                    GiveWagon(player, 1);
                    return false;
                }
                return true;
            }

            private bool IsValidConstruction(
                ConstructionMatrix matrix,
                BaseEntity baseEntity,
                List<BuildingBlock> buildingBlocks,
                BasePlayer player,
                Wagon wagon
            )
            {
                matrix.Init(baseEntity, buildingBlocks, player);
                if (!matrix.isValid)
                {
                    NotifyManager.SendMessageToPlayer(
                        player,
                        "NotValidConstruction",
                        ownIns._config.NotifyConfig.prefix
                    );
                    if (wagon == null)
                    {
                        GiveWagon(player, 1);
                    }

                    return false;
                }
                return true;
            }

            private static Wagon GetNewWagon(
                BasePlayer player,
                ConstructionMatrix matrix,
                Garage garage
            )
            {
                if (!ownIns.amountWagons.TryGetValue(player, out int value))
                {
                    value = 0;
                    ownIns.amountWagons.Add(player, value);
                }

                ownIns.amountWagons[player] = ++value;
                Wagon wagon = new();
                wagon.Init(player, matrix.posForSpawnWagon, garage);
                return wagon;
            }

            private bool IsTargetEntityBelongWagon(BaseEntity targetEntity, out Wagon wagon)
            {
                return allBaseEntityByWagons.TryGetValue(targetEntity.net.ID.Value, out wagon);
            }

            private void CheckWagonOnCustomMonument(
                BasePlayer player,
                BaseEntity baseEntity,
                BaseEntity targetEntity,
                Wagon wagon
            )
            {
                if (
                    playersOnMonuments.TryGetValue(player, out CustomMonument monument1)
                    && !monument1.IsWagonOnMonument(wagon.trainCar)
                )
                {
                    return;
                }

                if (monument1 == null)
                {
                    NotifyManager.SendMessageToPlayer(
                        player,
                        "NotOnBaseAndNotOnMonument",
                        ownIns._config.NotifyConfig.prefix
                    );
                    CancelAction(baseEntity, player);
                }
                else if (monument1.isBuildingState)
                {
                    wagon.CheckNewEntity(baseEntity, targetEntity, player);
                }
                else
                {
                    NotifyManager.SendMessageToPlayer(
                        player,
                        "ModeIsNotBuilding",
                        ownIns._config.NotifyConfig.prefix
                    );
                    CancelAction(baseEntity, player);
                }
            }

            private void DetermineAllMonumentsForWagon()
            {
                if (customMonuments.Count > 0)
                {
                    return;
                }

                foreach (BaseNetworkable entity in BaseNetworkable.serverEntities)
                {
                    ElectricalBranch branch = entity as ElectricalBranch;
                    if (branch == null || branch.branchAmount != 888444)
                    {
                        continue;
                    }

                    CustomMonument monument = new GameObject().AddComponent<CustomMonument>();
                    monument.Init(branch);
                    _ = customMonuments.Add(monument);
                }
            }

            private void DestroyStandsAndMods()
            {
                foreach (Stand stand in allStands.Values.ToHashSet())
                {
                    stand.Destroy();
                }

                foreach (Wagon wagon in wagons.ToHashSet())
                {
                    wagon.DestroyModification();
                }
            }

            private void DestroyAllCustomMonuments()
            {
                foreach (CustomMonument monument in customMonuments)
                {
                    monument.Destroy();
                    UnityEngine.Object.Destroy(monument.gameObject);
                }
            }

            private void DetermineSettingsEntities()
            {
                foreach (AcceptableEntities entity in ownIns._config.AcceptableEntities)
                {
                    acceptableEntities.Add(
                        entity.ShortName,
                        new SettingsEntity { IsAllowPut = entity.IsAllowPut, Amount = entity.Amount }
                    );
                }
            }

            private static void CopySerializableFields<T>(T src, T dst)
            {
                // ВНИМАНИЕ: reflection запрещён. Необходимо реализовать ручное копирование для используемых типов.
                // Пример для DoorCloser -> ModificationsWagon и TrainCar -> BaseCombatEntity
                // Здесь оставляем пустым или реализуем для конкретных случаев ниже.
            }

            private static Quaternion GetGlobalRot(Transform Transform, Vector3 localRotation)
            {
                return Transform.rotation * Quaternion.Euler(localRotation);
            }

            private static Vector3 GetGlobalPos(Transform Transform, Vector3 localPosition)
            {
                return Transform.TransformPoint(localPosition);
            }

            private static void GiveWagon(BasePlayer player, int amount)
            {
                Item item = ItemManager.CreateByName("electric.flasherlight", amount, SkindIdFlasher);
                item.name = "Wagon";
                MoveItem(player, item);
            }

            private static void CancelAction(BaseEntity entity, BasePlayer player)
            {
                if (entity is BuildingBlock block)
                {
                    ReturnResourcesToPlayer(block, player);
                }
                else
                {
                    ReturnEntityToPlayer(entity, player);
                }

                if (entity.IsExists())
                {
                    entity.Kill();
                }
            }

            private static void ReturnResourcesToPlayer(BuildingBlock buildingBlock, BasePlayer player)
            {
                foreach (ItemAmount itemAmount in buildingBlock.BuildCost())
                {
                    Item item = ItemManager.CreateByItemID(itemAmount.itemid, (int)itemAmount.amount);
                    MoveItem(player, item);
                }
            }

            private static void ReturnEntityToPlayer(BaseEntity entity, BasePlayer player)
            {
                BaseCombatEntity baseCombatEntity = entity as BaseCombatEntity;
                string strName = GetItemName(baseCombatEntity);
                Item item = ItemManager.CreateByName(strName, 1, entity.skinID);
                MoveItem(player, item);
            }

            private static string GetItemName(BaseCombatEntity baseCombatEntity)
            {
                if (baseCombatEntity.pickup.itemTarget == null)
                {
                    return ownIns.entityToItem.TryGetValue(
                        baseCombatEntity.PrefabName,
                        out string strName
                    )
                        ? strName
                        : GetShortNameItem(baseCombatEntity);
                }
                return baseCombatEntity.pickup.itemTarget.shortname;
            }

            private static string? GetShortNameItem(BaseCombatEntity baseCombatEntity)
            {
                foreach (ItemDefinition itemDef in ItemManager.GetItemDefinitions())
                {
                    if (itemDef == null)
                    {
                        continue;
                    }

                    ItemModDeployable itemMod = itemDef.GetComponent<ItemModDeployable>();
                    if (itemMod == null || itemMod.entityPrefab == null)
                    {
                        continue;
                    }

                    string path = itemMod.entityPrefab.resourcePath;
                    if (ownIns.entityToItem.ContainsKey(path))
                    {
                        continue;
                    }

                    if (path == baseCombatEntity.PrefabName)
                    {
                        ownIns.entityToItem.Add(path, itemDef.shortname);
                        return itemDef.shortname;
                    }
                }

                return null;
            }

            private static bool IsValidVariablesForBase(
                BasePlayer player,
                BaseEntity entity,
                out BuildingPrivlidge cupboard
            )
            {
                cupboard = null;

                if (!player.IsPlayer())
                {
                    return false;
                }

                if (entity == null)
                {
                    return false;
                }

                cupboard = player.GetBuildingPrivilege();
                if (cupboard == null)
                {
                    NotifyManager.SendMessageToPlayer(
                        player,
                        "BaseHasntCupboard",
                        ownIns._config.NotifyConfig.prefix
                    );
                    return false;
                }
                if (cupboard?.IsAuthed(player) == false)
                {
                    NotifyManager.SendMessageToPlayer(
                        player,
                        "NotAuthedInCupboard",
                        ownIns._config.NotifyConfig.prefix
                    );
                    return false;
                }

                return true;
            }

            private static HashSet<T> GetEntities<T>(Vector3 position, float radius, int layerMask = -1)
                where T : BaseEntity
            {
                HashSet<T> result = new();
                List<T> list = Pool.Get<List<T>>();
                Vis.Entities(position, radius, list, layerMask);
                foreach (T entity in list)
                {
                    _ = result.Add(entity);
                }

                Pool.FreeUnmanaged(ref list);
                return result;
            }

            public static void Debug(object format)
            {
                ownIns.Puts(format.ToString());
            }

            public static void DebugError(object format)
            {
                ownIns.PrintError(format.ToString());
            }

            public static void DebugWarning(object format)
            {
                ownIns.PrintWarning(format.ToString());
            }

            #endregion Helpers

            #region Variables

            [PluginReference]
            private readonly Plugin? GUIAnnouncements,
                DiscordMessages,
                Notify,
                MadMappersPluginUpdate,
                ArmoredTrain;

            private HashSet<WagonData> dataForWagons { get; set; } = new();
            private HashSet<GarageData> dataForGarages { get; set; } = new();

            private Dictionary<string, SettingsEntity> acceptableEntities { get; } = new();

            private HashSet<Wagon> wagons { get; } = new();
            private HashSet<CodeLock> allCodeLocks { get; } = new();
            private HashSet<RFReceiver> allReceivers { get; } = new();
            private HashSet<RANDSwitch> allSwitches { get; } = new();

            private HashSet<BasePlayer> callers { get; } = new();
            private HashSet<Wagon> wagonsBeingCalled { get; } = new();
            private HashSet<BasePlayer> playersWhoWantsToRemoveBaseWagon { get; } = new();

            private Dictionary<ulong, Garage> allFoundationsUnderWagons { get; } = new();
            private Dictionary<ulong, Stand> allStands { get; } = new();
            private Dictionary<ulong, Wagon> allBaseEntityByWagons { get; } = new();
            private Dictionary<ulong, Wagon> allTrainCars { get; } = new();
            private Dictionary<ulong, Wagon> allParents { get; } = new();

            private Dictionary<BuildingPrivlidge, Garage> dicGarages { get; } = new();
            private Dictionary<BasePlayer, int> amountWagons { get; } = new();

            private HashSet<CustomMonument> customMonuments { get; } = new();
            private Dictionary<PressButton, CustomMonument> buttonsOnMonuments { get; } = new();
            private Dictionary<BasePlayer, CustomMonument> playersOnMonuments { get; } = new();
            private Dictionary<ulong, Wagon> doorCloserToWagon { get; } = new();
            private HashSet<ulong> allObjectsOfWalls { get; } = new();

            private static TrainHomes? ownIns { get; set; }
            private bool onServerInit { get; set; }

            private Dictionary<string, string> entityToItem { get; } = new();

            private static readonly HashSet<ulong> skinIdFromCustomEntity = new() { 1594245394 };

            private static ulong SkindIdFlasher { get; } = 3171237719;
            private static int delayTime { get; } = 20;

            #endregion Variables

            #region Classes

            private static class LogManager
            {
                private static string? fileName;
                private static string? playerResult;
                private static string? damageResult;
                private static string? newHhp;
                private static string? owner;

                internal static void AddLogDamage(BaseCombatEntity baseCombatEntity, HitInfo info)
                {
                    ulong netId = baseCombatEntity.net.ID.Value;

                    if (
                        (
                            ownIns.allParents.TryGetValue(netId, out Wagon wagon)
                            || ownIns.allTrainCars.TryGetValue(netId, out wagon)
                        )
                        && wagon?.owner != null
                    )
                    {
                        owner = wagon.owner.displayName;
                    }
                    else
                    {
                        return;
                    }

                    string text = AddTimeToText();
                    damageResult =
                        info.damageTypes.GetMajorityDamageType() == Rust.DamageType.Decay
                            ? "decay"
                            : info.damageTypes.Total().ToString();
                    BasePlayer attacker = info.InitiatorPlayer;
                    if (attacker != null)
                    {
                        playerResult = attacker.displayName;
                    }
                    else if (info.Initiator != null)
                    {
                        playerResult = info.Initiator.PrefabName;
                    }
                    else
                    {
                        playerResult = "Not Found";
                    }

                    newHhp = baseCombatEntity.health.ToString();

                    fileName = baseCombatEntity.ShortPrefabName;
                    text +=
                        $"Wagon: {netId}; Damage: {damageResult}; Attacker: {playerResult}; NewHP: {newHhp}; Owner: {owner}";
                    AddLog(fileName, text);
                }

                private static string AddTimeToText()
                {
                    return $"[{DateTime.Now.ToShortTimeString()}] ";
                }

                private static void AddLog(string FileName, string text)
                {
                    ownIns.LogToFile(FileName, text, ownIns);
                }
            }

            private static class PluginUpdateManager
            {
                private const int badRequest = 400;
                private const int pluginNotFound = 404;
                private const int internalErrorFromWebServer = 500;

                private static readonly bool debug;

                private static bool IsValidAnswer(int code)
                {
                    if (code is 200 or 204)
                    {
                        return true;
                    }

                    if (debug)
                    {
                        DebugError(code);
                    }

                    return false;
                }

                internal static void CheckPluginUpdate()
                {
                    if (ownIns.MadMappersPluginUpdate != null)
                    {
                        return;
                    }

                    string UrlToApi =
                        $"https://madmappers.store/api/plugins/getversion?pluginName={ownIns.Title}";
                    ownIns.webrequest.Enqueue(
                        UrlToApi,
                        null,
                        (code, response) =>
                        {
                            if (!IsValidAnswer(code))
                            {
                                return;
                            }

                            string[] parts = response.Split('.');
                            VersionNumber versionNumber = new(
                                int.Parse(parts[0]),
                                int.Parse(parts[1]),
                                int.Parse(parts[2])
                            );
                            if (versionNumber > ownIns.Version)
                            {
                                ownIns.PrintWarning(
                                    $"Your version of the plugin is outdated, upgrade to the version {response}"
                                );
                            }
                        },
                        ownIns,
                        RequestMethod.GET
                    );
                }

                private static void DebugError(int code)
                {
                    switch (code)
                    {
                        case badRequest:
                            ownIns.PrintWarning("Bad Request");
                            break;
                        case pluginNotFound:
                            ownIns.PrintWarning("Plugin Not Found");
                            break;
                        case internalErrorFromWebServer:
                            ownIns.PrintWarning("Internal Error From Web Server");
                            break;
                    }
                }
            }

            private static class PermissionManager
            {
                internal const string giveWagon = "trainhomes.givewagon";
                internal const string freeWagons = "trainhomes.showfreewagons";

                private static PluginConfig config => ownIns._config;
                private static Permission perm => ownIns.permission;

                internal static void RegisterPermissions()
                {
                    string message = "";

                    if (config.PermissionsForAmountWagons.Count == 0)
                    {
                        return;
                    }

                    foreach (KeyValuePair<string, int> kvp in config.PermissionsForAmountWagons)
                    {
                        perm.RegisterPermission(kvp.Key, ownIns);
                        message += $"'{kvp.Key}' ";
                    }

                    perm.RegisterPermission(giveWagon, ownIns);
                    perm.RegisterPermission(freeWagons, ownIns);

                    message += $"'{giveWagon}' '{freeWagons}'";
                    ownIns.PrintWarning($"Permissions {message} has been registered.");
                }
            }

            private static class GUIManager
            {
                private const string invisRGBA = "0 0 0 0";

                private const string constAnchor = "0.5 0.5";
                private const string strdParent = "Hud.Menu";
                private const string strdNameCuiPanel = "InvisBackground";
                private const string closeGui = "Close_GUI";

                private const string hexWhite = "#ffffff";
                private const string hexRed = "#d10000";

                private const string hex1 = "#857d73";
                private const string hex2 = "#ff8c00";

                internal enum GUITopic
                {
                    Base = 0,
                    General = 1,
                    Construction = 2,
                    CustomMonument = 3,
                }

                internal static void CreateGUI(BasePlayer player, GUITopic topic)
                {
                    DestroyGUI(player);
                    CuiElementContainer container = new();

                    AddStandardCuiPanel(container);
                    AddNewCuiElement(
                        container,
                        "Background",
                        "InvisBackground",
                        "#1c2933",
                        "-448 -250",
                        "448 250"
                    );
                    AddNewCuiButton(
                        container,
                        "Button1",
                        "Background",
                        closeGui,
                        "[X]",
                        hexWhite,
                        hexRed,
                        TextAnchor.MiddleCenter,
                        24,
                        "400 202",
                        "448 250"
                    );

                    switch (topic)
                    {
                        case GUITopic.Base:
                            CreateBaseGUI(container, player);
                            break;
                        case GUITopic.General:
                            CreateGeneralGUI(container, player);
                            break;
                        case GUITopic.Construction:
                            CreateConstructionGUI(container, player);
                            break;
                        case GUITopic.CustomMonument:
                            break;
                        default:
                            CreateCustomMonumentGUI(container, player);
                            break;
                    }

                    _ = CuiHelper.AddUi(player, container);
                }

                private static void CreateBaseGUI(CuiElementContainer container, BasePlayer player)
                {
                    AddNewCuiButton(
                        container,
                        "Button2",
                        "Background",
                        "GUI_General",
                        GetMessage("GUI_General", player.UserIDString),
                        hexWhite,
                        hex1,
                        TextAnchor.MiddleCenter,
                        24,
                        "-448 202",
                        "-236 250"
                    );

                    AddNewCuiButton(
                        container,
                        "Button3",
                        "Background",
                        "GUI_Construction",
                        GetMessage("GUI_Construction", player.UserIDString),
                        hexWhite,
                        hex1,
                        TextAnchor.MiddleCenter,
                        24,
                        "-236 202",
                        "-24 250"
                    );

                    AddNewCuiButton(
                        container,
                        "Button4",
                        "Background",
                        "GUI_CustomMonument",
                        GetMessage("GUI_CustomMonument", player.UserIDString),
                        hexWhite,
                        hex1,
                        TextAnchor.MiddleCenter,
                        24,
                        "-24 202",
                        "188 250"
                    );
                }

                private static void CreateGeneralGUI(CuiElementContainer container, BasePlayer player)
                {
                    AddNewCuiButton(
                        container,
                        "Button2",
                        "Background",
                        "GUI_General",
                        GetMessage("GUI_General", player.UserIDString),
                        hexWhite,
                        hex2,
                        TextAnchor.MiddleCenter,
                        24,
                        "-448 202",
                        "-236 250"
                    );

                    AddNewCuiButton(
                        container,
                        "Button3",
                        "Background",
                        "GUI_Construction",
                        GetMessage("GUI_Construction", player.UserIDString),
                        hexWhite,
                        hex1,
                        TextAnchor.MiddleCenter,
                        24,
                        "-236 202",
                        "-24 250"
                    );

                    AddNewCuiButton(
                        container,
                        "Button4",
                        "Background",
                        "GUI_CustomMonument",
                        GetMessage("GUI_CustomMonument", player.UserIDString),
                        hexWhite,
                        hex1,
                        TextAnchor.MiddleCenter,
                        24,
                        "-24 202",
                        "188 250"
                    );

                    AddNewInvisCuiElement(
                        container,
                        "InvisBackgroundForText",
                        "Background",
                        "-424 -178",
                        "424 178"
                    );

                    AddNewCuiText(
                        container,
                        "Text",
                        "InvisBackgroundForText",
                        GetMessage("GUI_General_Description", player.UserIDString),
                        hexWhite,
                        TextAnchor.UpperLeft,
                        18
                    );
                }

                private static void CreateConstructionGUI(
                    CuiElementContainer container,
                    BasePlayer player
                )
                {
                    AddNewCuiButton(
                        container,
                        "Button2",
                        "Background",
                        "GUI_General",
                        GetMessage("GUI_General", player.UserIDString),
                        hexWhite,
                        hex1,
                        TextAnchor.MiddleCenter,
                        24,
                        "-448 202",
                        "-236 250"
                    );

                    AddNewCuiButton(
                        container,
                        "Button3",
                        "Background",
                        "GUI_Construction",
                        GetMessage("GUI_Construction", player.UserIDString),
                        hexWhite,
                        hex2,
                        TextAnchor.MiddleCenter,
                        24,
                        "-236 202",
                        "-24 250"
                    );

                    AddNewCuiButton(
                        container,
                        "Button4",
                        "Background",
                        "GUI_CustomMonument",
                        GetMessage("GUI_CustomMonument", player.UserIDString),
                        hexWhite,
                        hex1,
                        TextAnchor.MiddleCenter,
                        24,
                        "-24 202",
                        "188 250"
                    );

                    AddNewInvisCuiElement(
                        container,
                        "InvisBackgroundForText",
                        "Background",
                        "-424 -178",
                        "424 178"
                    );

                    AddNewCuiText(
                        container,
                        "Text",
                        "InvisBackgroundForText",
                        GetMessage("GUI_Construction_Description", player.UserIDString),
                        hexWhite,
                        TextAnchor.UpperLeft,
                        18
                    );
                }

                private static void CreateCustomMonumentGUI(
                    CuiElementContainer container,
                    BasePlayer player
                )
                {
                    AddNewCuiButton(
                        container,
                        "Button2",
                        "Background",
                        "GUI_General",
                        GetMessage("GUI_General", player.UserIDString),
                        hexWhite,
                        hex1,
                        TextAnchor.MiddleCenter,
                        24,
                        "-448 202",
                        "-236 250"
                    );

                    AddNewCuiButton(
                        container,
                        "Button3",
                        "Background",
                        "GUI_Construction",
                        GetMessage("GUI_Construction", player.UserIDString),
                        hexWhite,
                        hex1,
                        TextAnchor.MiddleCenter,
                        24,
                        "-236 202",
                        "-24 250"
                    );

                    AddNewCuiButton(
                        container,
                        "Button4",
                        "Background",
                        "GUI_CustomMonument",
                        GetMessage("GUI_CustomMonument", player.UserIDString),
                        hexWhite,
                        hex2,
                        TextAnchor.MiddleCenter,
                        24,
                        "-24 202",
                        "188 250"
                    );

                    AddNewInvisCuiElement(
                        container,
                        "InvisBackgroundForText",
                        "Background",
                        "-424 -178",
                        "424 178"
                    );

                    AddNewCuiText(
                        container,
                        "Text",
                        "InvisBackgroundForText",
                        GetMessage("GUI_CustomMonument_Description", player.UserIDString),
                        hexWhite,
                        TextAnchor.UpperLeft,
                        18
                    );
                }

                private static void AddStandardCuiPanel(
                    CuiElementContainer container,
                    bool cursorEnabled = false
                )
                {
                    _ = container.Add(
                        new CuiPanel
                        {
                            Image = { Color = invisRGBA },
                            RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" },
                            CursorEnabled = cursorEnabled,
                        },
                        strdParent,
                        strdNameCuiPanel
                    );
                }

                private static void AddNewInvisCuiElement(
                    CuiElementContainer container,
                    string name,
                    string parent,
                    string offsetMin,
                    string offsetMax
                )
                {
                    container.Add(
                        new CuiElement
                        {
                            Name = name,
                            Parent = parent,
                            Components =
                            {
                            new CuiImageComponent { Color = invisRGBA },
                            new CuiRectTransformComponent
                            {
                                AnchorMin = constAnchor,
                                AnchorMax = constAnchor,
                                OffsetMin = offsetMin,
                                OffsetMax = offsetMax,
                            },
                            },
                        }
                    );
                }

                private static void AddNewCuiElement(
                    CuiElementContainer container,
                    string name,
                    string parent,
                    string hexColor,
                    string offsetMin,
                    string offsetMax
                )
                {
                    container.Add(
                        new CuiElement
                        {
                            Name = name,
                            Parent = parent,
                            Components =
                            {
                            new CuiImageComponent { Color = ConvertHexToRGBA(hexColor) },
                            new CuiRectTransformComponent
                            {
                                AnchorMin = constAnchor,
                                AnchorMax = constAnchor,
                                OffsetMin = offsetMin,
                                OffsetMax = offsetMax,
                            },
                            },
                        }
                    );
                }

                private static void AddNewCuiText(
                    CuiElementContainer container,
                    string name,
                    string parent,
                    string text,
                    string hexColorText,
                    TextAnchor align,
                    int fontSize
                )
                {
                    container.Add(
                        new CuiElement
                        {
                            Name = name,
                            Parent = parent,
                            Components =
                            {
                            new CuiTextComponent()
                            {
                                Color = ConvertHexToRGBA(hexColorText),
                                Text = text,
                                Align = align,
                                FontSize = fontSize,
                            },
                            new CuiRectTransformComponent { AnchorMin = "0 0", AnchorMax = "1 1" },
                            },
                        }
                    );
                }

                private static void AddNewCuiButton(
                    CuiElementContainer container,
                    string name,
                    string parent,
                    string command,
                    string text,
                    string hexColorText,
                    string hexColor,
                    TextAnchor align,
                    int fontSize,
                    string offsetMin,
                    string offsetMax
                )
                {
                    _ = container.Add(
                        new CuiButton
                        {
                            RectTransform =
                            {
                            AnchorMin = constAnchor,
                            AnchorMax = constAnchor,
                            OffsetMin = offsetMin,
                            OffsetMax = offsetMax,
                            },
                            Button = { Command = command, Color = ConvertHexToRGBA(hexColor) },
                            Text =
                            {
                            Text = text,
                            Color = ConvertHexToRGBA(hexColorText),
                            Align = align,
                            FontSize = fontSize,
                            },
                        },
                        parent,
                        name
                    );
                }

                internal static void DestroyGUI(BasePlayer player)
                {
                    _ = CuiHelper.DestroyUi(player, "InvisBackground");
                }

                private static string? ConvertHexToRGBA(string hexColor)
                {
                    if (!ColorUtility.TryParseHtmlString(hexColor, out Color color))
                    {
                        return null;
                    }

                    float red = color.r;
                    float green = color.g;
                    float blue = color.b;
                    const float alpha = 1f;

                    return $"{red} {green} {blue} {alpha}";
                }
            }

            private static class NotifyManager
            {
                internal static void PrintInfoMessage(
                    BasePlayer player,
                    string langKey,
                    params object[] args
                )
                {
                    if (player == null)
                    {
                        ownIns.PrintWarning(ClearColorAndSize(GetMessage(langKey, null, args)));
                    }
                    else
                    {
                        ownIns.PrintToChat(player, GetMessage(langKey, player.UserIDString, args));
                    }
                }

                internal static void PrintError(BasePlayer player, string langKey, params object[] args)
                {
                    if (player == null)
                    {
                        ownIns.PrintError(ClearColorAndSize(GetMessage(langKey, null, args)));
                    }
                    else
                    {
                        ownIns.PrintToChat(player, GetMessage(langKey, player.UserIDString, args));
                    }
                }

                internal static void PrintLogMessage(string langKey, params object[] args)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] is int)
                        {
                            args[i] = GetTimeMessage(null, (int)args[i]);
                        }
                    }

                    ownIns.Puts(ClearColorAndSize(GetMessage(langKey, null, args)));
                }

                internal static void PrintWarningMessage(string langKey, params object[] args)
                {
                    ownIns.PrintWarning(ClearColorAndSize(GetMessage(langKey, null, args)));
                }

                private static string ClearColorAndSize(string message)
                {
                    message = message
                        .Replace("</color>", string.Empty)
                        .Replace("</size>", string.Empty);
                    while (message.Contains("<color="))
                    {
                        int index = message.IndexOf("<color=");
                        message = message.Remove(index, message.IndexOf(">", index) - index + 1);
                    }
                    while (message.Contains("<size="))
                    {
                        int index = message.IndexOf("<size=");
                        message = message.Remove(index, message.IndexOf(">", index) - index + 1);
                    }
                    return message;
                }

                internal static void SendMessageToAll(string langKey, params object[] args)
                {
                    foreach (BasePlayer player in BasePlayer.activePlayerList)
                    {
                        if (player != null)
                        {
                            SendMessageToPlayer(player, langKey, args);
                        }
                    }

                    TrySendDiscordMessage(langKey, args);
                }

                internal static void SendMessageToPlayer(
                    BasePlayer player,
                    string langKey,
                    params object[] args
                )
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] is int)
                        {
                            args[i] = GetTimeMessage(player.UserIDString, (int)args[i]);
                        }
                    }

                    string playerMessage = GetMessage(langKey, player.UserIDString, args);

                    if (ownIns._config.NotifyConfig.chatConfig.isEnabled)
                    {
                        ownIns.PrintToChat(player, playerMessage);
                    }

                    if (ownIns._config.NotifyConfig.gameTipConfig.isEnabled)
                    {
                        player.SendConsoleCommand(
                            "gametip.showtoast",
                            ownIns._config.NotifyConfig.gameTipConfig.style,
                            ClearColorAndSize(playerMessage)
                        );
                    }

                    if (
                        ownIns._config.NotifyConfig.guiAnnouncementsConfig.isEnabled
                        && ownIns.plugins.Exists("GUIAnnouncements")
                    )
                    {
                        _ = (
                            ownIns.GUIAnnouncements?.Call(
                                "CreateAnnouncement",
                                ClearColorAndSize(playerMessage),
                                ownIns._config.NotifyConfig.guiAnnouncementsConfig.bannerColor,
                                ownIns._config.NotifyConfig.guiAnnouncementsConfig.textColor,
                                player,
                                ownIns._config.NotifyConfig.guiAnnouncementsConfig.apiAdjustVPosition
                            )
                        );
                    }

                    if (
                        ownIns._config.NotifyConfig.notifyPluginConfig.isEnabled
                        && ownIns.plugins.Exists("Notify")
                    )
                    {
                        _ = (
                            ownIns.Notify?.Call(
                                "SendNotify",
                                player,
                                ownIns._config.NotifyConfig.notifyPluginConfig.type,
                                ClearColorAndSize(playerMessage)
                            )
                        );
                    }
                }

                private static string GetTimeMessage(string userIDString, int seconds)
                {
                    string message = "";

                    TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
                    if (timeSpan.Hours > 0)
                    {
                        message += $" {timeSpan.Hours} {GetMessage("Hours", userIDString)}";
                    }

                    if (timeSpan.Minutes > 0)
                    {
                        message += $" {timeSpan.Minutes} {GetMessage("Minutes", userIDString)}";
                    }

                    if (message?.Length == 0)
                    {
                        message += $" {timeSpan.Seconds} {GetMessage("Seconds", userIDString)}";
                    }

                    return message;
                }

                private static void TrySendDiscordMessage(string langKey, params object[] args)
                {
                    if (CanSendDiscordMessage(langKey))
                    {
                        for (int i = 0; i < args.Length; i++)
                        {
                            if (args[i] is int)
                            {
                                args[i] = GetTimeMessage(null, (int)args[i]);
                            }
                        }

                        object fields = new[]
                        {
                        new
                        {
                            name = ownIns.Title,
                            value = ClearColorAndSize(GetMessage(langKey, null, args)),
                            inline = false,
                        },
                    };
                        _ = (
                            ownIns.DiscordMessages?.Call(
                                "API_SendFancyMessage",
                                ownIns._config.NotifyConfig.discordMessagesConfig.webhookUrl,
                                "",
                                ownIns._config.NotifyConfig.discordMessagesConfig.embedColor,
                                JsonConvert.SerializeObject(fields),
                                null,
                                ownIns
                            )
                        );
                    }
                }

                private static bool CanSendDiscordMessage(string langKey)
                {
                    return ownIns._config.NotifyConfig.discordMessagesConfig.keys.Contains(langKey)
                        && ownIns._config.NotifyConfig.discordMessagesConfig.isEnabled
                        && !string.IsNullOrEmpty(
                            ownIns._config.NotifyConfig.discordMessagesConfig.webhookUrl
                        )
                        && ownIns._config.NotifyConfig.discordMessagesConfig.webhookUrl
                            != "https://support.discordapp.com/hc/en-us/articles/228383668-Intro-to-Webhooks";
                }
            }

            public sealed class WagonData
            {
                public ulong idPlayer;
                public ulong idParent;
                public ulong idTrainCar;

                public string? code;
                public int frequency;

                public string? pos;
                public string? rot;

                public HashSet<ulong>? authPlayers;
            }

            public sealed class GarageData
            {
                public ulong idCupboard;
                public HashSet<PosInGarageData> poss { get; set; } = new();
            }

            public sealed class PosInGarageData
            {
                public string? pos;
                public string? rot;
                public ulong idParent;
                public HashSet<ulong> foundations { get; set; } = new();
            }

            internal sealed class SettingsEntity
            {
                public bool IsAllowPut { get; init; }
                public int Amount { get; init; }
            }

            internal sealed class BaseBarricade : BaseCombatEntity { }

            internal sealed class Wagon
            {
                internal BasePlayer? owner { get; private set; }

                private BaseEntity? wagonToMove { get; set; }
                internal DoorCloser? parent { get; private set; }
                internal TrainCar? trainCar { get; private set; }

                internal Garage? garage { get; set; }
                internal bool freeWagon { get; private set; }
                internal bool inGarage { get; private set; }
                internal bool onCustomMonument { get; private set; }
                private ModificationsWagon? modifications { get; set; }
                private Stand? stand { get; set; }

                private uint newBuildingID { get; } = BuildingManager.server.NewBuildingID();
                internal HashSet<BuildingBlock> allBuilidngBlocks { get; } = new();
                private Dictionary<BaseEntity, BaseEntity> allNotBuilidngBlocks { get; } = new();

                private KeyValuePair<Vector3, Quaternion> posInGarage { get; set; }

                private static float localFloorHeight1 { get; } = 0.188f;
                private static float localFloorHeight2 { get; } = 1.52f;

                private static HashSet<string> localPosFloorOnWagon { get; } =
                    new()
                    {
                    "(0, 0.188, -6)",
                    "(0, 0.188, -3)",
                    "(0, 0.188, 0)",
                    "(0, 0.188, 3)",
                    "(0, 0.188, 6)",
                    };

                internal void Init(
                    BasePlayer player,
                    KeyValuePair<Vector3, Quaternion> pos,
                    Garage Garage
                )
                {
                    _ = ownIns.wagons.Add(this);
                    owner = player;
                    posInGarage = pos;
                    garage = Garage;

                    StartCreateOnBase();
                }

                internal void InitFreeWagon(Vector3 pos, Quaternion rot)
                {
                    owner = null;
                    posInGarage = new KeyValuePair<Vector3, Quaternion>();
                    garage = null;

                    StartCreateFreeWagon(pos, rot);
                }

                internal void InitWithData(WagonData data, ref bool isValidData)
                {
                    posInGarage = new KeyValuePair<Vector3, Quaternion>(
                        data.pos.ToVector3(),
                        Quaternion.Euler(data.rot.ToVector3())
                    );

                    if (data.idTrainCar != 0)
                    {
                        InitUsingTrainCar(data, ref isValidData);
                    }
                    else
                    {
                        InitUsingDoorCloser(data, ref isValidData);
                    }

                    if (!isValidData)
                    {
                        return;
                    }

                    if (!IsExistOwnerOnServer(data.idPlayer.ToString(), ref isValidData))
                    {
                        return;
                    }

                    SpawnModifications(data);
                    if (!ownIns.amountWagons.TryGetValue(owner, out int value))
                    {
                        value = 0;
                        ownIns.amountWagons.Add(owner, value);
                    }

                    ownIns.amountWagons[owner] = ++value;
                    garage = null;
                }

                private void InitUsingTrainCar(WagonData data, ref bool isValidData)
                {
                    trainCar =
                        BaseNetworkable.serverEntities.FirstOrDefault(
                            p => p.net.ID.Value == data.idTrainCar
                        ) as TrainCar;
                    if (trainCar == null)
                    {
                        isValidData = false;
                        return;
                    }
                    InitChildrenWithData(trainCar);
                    InitAllNotBuildingBlocks();
                    ChangeParametersTrainCar();
                    if (!ownIns.allTrainCars.ContainsKey(trainCar.net.ID.Value))
                    {
                        ownIns.allTrainCars.Add(trainCar.net.ID.Value, this);
                    }
                }

                private void InitUsingDoorCloser(WagonData data, ref bool isValidData)
                {
                    Debug("Search for door closer...");
                    parent =
                        Enumerable.FirstOrDefault(
                            BaseNetworkable.serverEntities,
                            p => p.IsExists() && p.net != null && p.net.ID.Value == data.idParent
                        ) as DoorCloser
                        ?? SearchDoorCloser(data);

                    if (parent == null)
                    {
                        Debug("door closer == null");
                        isValidData = false;
                        return;
                    }
                    Debug("door closer != null");
                    RecoveryBaseWagon();
                }

                private DoorCloser SearchDoorCloser(WagonData data)
                {
                    DoorCloser doorCloser = null;
                    foreach (DoorCloser closer in GetEntities<DoorCloser>(data.pos.ToVector3(), 2f))
                    {
                        if (closer != null && closer.skinID == SkindIdFlasher)
                        {
                            doorCloser = closer;
                        }
                    }

                    return doorCloser;
                }

                private void RecoveryBaseWagon()
                {
                    InitChildrenWithData(parent);
                    InitAllNotBuildingBlocks();

                    stand = new Stand(parent, this);
                    //SetParentForBuildingBlocks(baseWagon);
                    //RemoveParentForNotBuildingBlock();
                    ownIns.doorCloserToWagon.Add(parent.net.ID.Value, this);
                    //objectForUnload.Kill();

                    inGarage = true;
                    if (!ownIns.allParents.ContainsKey(parent.net.ID.Value))
                    {
                        ownIns.allParents.Add(parent.net.ID.Value, this);
                    }
                }

                private void InitChildrenWithData(BaseEntity Parent)
                {
                    if (Parent.children.Count == 0)
                    {
                        return;
                    }

                    foreach (BaseEntity? child in Parent.children)
                    {
                        TryMakeEntityNotDecay(child);

                        if (child is not BuildingBlock block)
                        {
                            continue;
                        }

                        _ = allBuilidngBlocks.Add(block);
                        if (!ownIns.allBaseEntityByWagons.ContainsKey(block.net.ID.Value))
                        {
                            ownIns.allBaseEntityByWagons.Add(block.net.ID.Value, this);
                        }
                    }
                }

                private void InitAllNotBuildingBlocks()
                {
                    foreach (BuildingBlock block in allBuilidngBlocks)
                    {
                        if (block.children.Count == 0)
                        {
                            continue;
                        }

                        foreach (BaseEntity? child1 in block.children)
                        {
                            TryMakeEntityNotDecay(child1);
                            if (!allNotBuilidngBlocks.ContainsKey(child1))
                            {
                                allNotBuilidngBlocks.Add(child1, block);
                            }

                            if (!ownIns.allBaseEntityByWagons.ContainsKey(child1.net.ID.Value))
                            {
                                ownIns.allBaseEntityByWagons.Add(child1.net.ID.Value, this);
                            }

                            if (child1.children.Count == 0)
                            {
                                continue;
                            }

                            foreach (BaseEntity? child2 in child1.children)
                            {
                                TryMakeEntityNotDecay(child2);
                                _ = allNotBuilidngBlocks.TryAdd(child2, child1);
                                if (!ownIns.allBaseEntityByWagons.ContainsKey(child2.net.ID.Value))
                                {
                                    ownIns.allBaseEntityByWagons.Add(child2.net.ID.Value, this);
                                }
                            }
                        }
                    }
                }

                private bool IsExistOwnerOnServer(string str, ref bool isValidData)
                {
                    owner = BasePlayer.FindAwakeOrSleeping(str);
                    if (owner == null)
                    {
                        isValidData = false;
                        return false;
                    }
                    return true;
                }

                private void StartCreateOnBase()
                {
                    SpawnParent();
                    stand = new Stand(parent, this);
                    SpawnModifications();
                    SpawnHouse();

                    inGarage = true;
                }

                private void StartCreateFreeWagon(Vector3 pos, Quaternion rot)
                {
                    SpawnTrainCar(pos, rot);
                    _ = ownIns.timer.In(
                        delayTime,
                        () =>
                        {
                            if (trainCar == null)
                            {
                                return;
                            }

                            ownIns.allTrainCars.Add(trainCar.net.ID.Value, this);

                            SpawnModifications();
                            SpawnHouse();

                            inGarage = false;
                            freeWagon = true;

                            _ = ownIns.wagons.Add(this);
                        }
                    );
                }

                private void SpawnParent()
                {
                    Vector3 pos = new(
                        posInGarage.Key.x,
                        posInGarage.Key.y + 0.2f + 1.332f,
                        posInGarage.Key.z
                    );
                    parent =
                        GameManager.server.CreateEntity(
                            "assets/prefabs/misc/doorcloser/doorcloser.prefab",
                            pos,
                            posInGarage.Value
                        ) as DoorCloser;
                    parent!.enableSaving = true;
                    parent.skinID = SkindIdFlasher;
                    parent.OwnerID = owner.userID;
                    parent.Spawn();

                    if (!ownIns.allParents.ContainsKey(parent.net.ID.Value))
                    {
                        ownIns.allParents.Add(parent.net.ID.Value, this);
                    }
                }

                private void SpawnModifications(WagonData? data = null)
                {
                    CreateModifications();
                    modifications.Spawn();

                    modifications.SetParent(GetActiveWagon, true, true);
                    if (owner != null)
                    {
                        modifications.Init(owner, this, data);
                    }
                    else
                    {
                        modifications.InitForFreeWagon(this);
                    }
                }

                private void CreateModifications()
                {
                    BaseEntity entity = GetActiveWagon;
                    Vector3 pos =
                        entity == parent
                            ? entity.transform.position
                            : entity.transform.position + new Vector3(0, 1.332f, 0);
                    Quaternion rot = entity.transform.rotation;

                    DoorCloser closer =
                        GameManager.server.CreateEntity(
                            "assets/prefabs/misc/doorcloser/doorcloser.prefab",
                            pos,
                            rot
                        ) as DoorCloser;

                    modifications = closer!.gameObject.AddComponent<ModificationsWagon>();

                    CopySerializableFields(closer, modifications);

                    UnityEngine.Object.DestroyImmediate(closer, true);
                    modifications.enableSaving = false;
                }

                private BaseEntity GetActiveWagon => parent ?? trainCar;

                private void SpawnHouse()
                {
                    SpawnFloorsOnWagon();
                }

                internal void MoveWagonInBase(
                    KeyValuePair<Vector3, Quaternion> newPosInGarage,
                    Garage newGarage
                )
                {
                    posInGarage = newPosInGarage;
                    garage = newGarage;
                    wagonToMove = trainCar;
                    SpawnParent();
                    stand = new Stand(parent, this);
                    ChangeParent(parent);
                    modifications.LimitNetworkingForBlocks();
                    inGarage = true;
                }

                internal void MoveWagonToTrack(Vector3 pos, BasePlayer caller)
                {
                    SpawnTrainCar(pos, new Quaternion());
                    _ = ownIns.timer.In(
                        100,
                        () =>
                        {
                            if (trainCar == null)
                            {
                                NotifyManager.SendMessageToPlayer(
                                    caller,
                                    "NotValidPosForTrainCar",
                                    ownIns._config.NotifyConfig.prefix
                                );
                                return;
                            }

                            wagonToMove = parent;
                            ownIns.allTrainCars.Add(trainCar.net.ID.Value, this);
                            ChangeParent(trainCar);

                            garage?.TryRemoveWagon(this);
                            inGarage = false;

                            _ = ownIns.timer.In(1000, () => modifications.LimitNetworkingForBlocks());
                        }
                    );
                }

                private void SpawnTrainCar(Vector3 pos, Quaternion rot)
                {
                    trainCar =
                        GameManager.server.CreateEntity(
                            "assets/content/vehicles/trains/wagons/trainwagonc.entity.prefab",
                            pos,
                            rot
                        ) as TrainCar;
                    trainCar!.Spawn();
                    ChangeParametersTrainCar();
                }

                private void ChangeParametersTrainCar()
                {
                    trainCar._maxHealth = ownIns._config.HpWagon;
                    trainCar.startHealth = ownIns._config.HpWagon;
                    trainCar.health = ownIns._config.HpWagon;
                    trainCar.CancelInvoke(trainCar.DecayTick);
                }

                private void ChangeParent(BaseEntity gParent)
                {
                    Vector3 newPos;
                    if (wagonToMove == trainCar)
                    {
                        UnityEngine.Object.DestroyImmediate(trainCar.platformParentTrigger);

                        newPos = new Vector3(
                            parent.transform.position.x,
                            parent.transform.position.y - 1.332f,
                            parent.transform.position.z
                        );
                        wagonToMove.transform.position = newPos;
                        wagonToMove.transform.rotation = parent.transform.rotation;
                    }
                    else
                    {
                        UpdateForNotBuildingBlocks();

                        newPos = new Vector3(
                            trainCar.transform.position.x,
                            trainCar.transform.position.y + 1.332f,
                            trainCar.transform.position.z
                        );
                        wagonToMove.transform.position = newPos;
                        wagonToMove.transform.rotation = trainCar.transform.rotation;
                    }

                    SetParentForBuildingBlocks(gParent);
                    modifications.SetParent(gParent, true, true);

                    _ = ownIns.timer.In(
                        delayTime,
                        () =>
                        {
                            if (wagonToMove == trainCar)
                            {
                                _ = ownIns.allTrainCars.Remove(trainCar.net.ID.Value);
                                trainCar.Kill();
                            }
                            else
                            {
                                RemoveEverythingLinkedToBaseWagon();
                            }
                        }
                    );
                }

                public void UpdateForNotBuildingBlocks()
                {
                    foreach (KeyValuePair<BaseEntity, BaseEntity> kvp in allNotBuilidngBlocks)
                    {
                        if (kvp.Key is IOEntity ioEntity)
                        {
                            UpdateIOEntity(ioEntity);
                            ioEntity.SetParent(kvp.Value, true, true);
                        }
                    }
                }

                private void UpdateIOEntity(IOEntity entity)
                {
                    //entity.SendIONetworkUpdate();
                    //entity.SendNetworkUpdate();
                    //entity.SendNetworkUpdateImmediate();
                    //entity.SendNetworkUpdate_Position();
                    if (entity.outputs.Length > 0)
                    {
                        UpdateOutputsOrInputs(entity.outputs);
                    }

                    if (entity.inputs.Length > 0)
                    {
                        UpdateOutputsOrInputs(entity.inputs);
                    }
                }

                private void UpdateOutputsOrInputs(IOEntity.IOSlot[] slots)
                {
                    foreach (IOEntity.IOSlot iORef in slots)
                    {
                        IOEntity iOEntity = iORef.connectedTo.ioEnt;

                        if (iOEntity == null)
                        {
                            continue;
                        }

                        if (!allNotBuilidngBlocks.ContainsKey(iOEntity))
                        {
                            iOEntity.ClearConnections();
                        }
                    }
                }

                private static void SetParentForNotBuildingBlock(
                    BaseEntity entity,
                    BaseEntity targetEntity
                )
                {
                    if (entity.GetParentEntity() != null)
                    {
                        return;
                    }

                    Vector3 localPos = targetEntity.transform.InverseTransformPoint(
                        entity.transform.position
                    );
                    Vector3 rot = entity.transform.rotation.eulerAngles;

                    entity.SetParent(targetEntity);

                    entity.transform.localPosition = localPos;
                    entity.transform.rotation = Quaternion.Euler(rot);
                }

                private void SetParentForBuildingBlocks(BaseEntity Parent)
                {
                    foreach (BuildingBlock buildingBlock in allBuilidngBlocks)
                    {
                        buildingBlock.SetParent(Parent, true, true);
                    }
                }

                private void RemoveEverythingLinkedToBaseWagon()
                {
                    _ = ownIns.allParents.Remove(parent.net.ID.Value);
                    stand.Destroy();
                    stand = null;
                    parent.Kill();
                }

                private void UnParentIOEntity()
                {
                    _ = ownIns.timer.In(
                        delayTime,
                        () =>
                        {
                            foreach (KeyValuePair<BaseEntity, BaseEntity> kvp in allNotBuilidngBlocks)
                            {
                                BaseEntity baseEntity = kvp.Key;
                                if (baseEntity is not IOEntity || IsIgnoredEntity(baseEntity))
                                {
                                    continue;
                                }

                                baseEntity.SetParent(null, true, true);
                                baseEntity.SendNetworkUpdate();
                            }
                        }
                    );
                }

                private static bool IsIgnoredEntity(BaseEntity entity)
                {
                    return entity
                        is DoorCloser
                            or KeyLock
                            or CodeLock
                            or DoorKnocker
                            or GrowableEntity
                            or IndustrialStorageAdaptor
                            or StorageMonitor
                            or IndustrialCrafter
                            or TorchDeployableLightSource
                            or TorchWeapon
                            or Door;
                }

                internal void ThrowAwayFlare(BasePlayer player)
                {
                    RoadFlare flare =
                        GameManager.server.CreateEntity(
                            "assets/prefabs/tools/flareold/flare.deployed.prefab",
                            player.transform.position + new Vector3(0f, 1f, 0f)
                        ) as RoadFlare;
                    flare!.Spawn();
                    Rigidbody rigidbody = flare.GetComponent<Rigidbody>();

                    Vector3 throwDirection = player.eyes.HeadForward();
                    throwDirection.y += 0.5f;
                    rigidbody.AddForce(throwDirection.normalized * 1.3f, ForceMode.Impulse);

                    SpawnerWagon spawner = flare.gameObject.AddComponent<SpawnerWagon>();
                    spawner.Init(this, player);
                }

                internal WagonData CreateInfoForData()
                {
                    WagonData wagonData = new()
                    {
                        idPlayer = owner.userID,
                        code = modifications.code,
                        frequency = modifications.frequency,

                        authPlayers = new HashSet<ulong>(),
                    };
                    foreach (ulong playerID in modifications.GetWhitelist())
                    {
                        _ = wagonData.authPlayers.Add(playerID);
                    }

                    if (inGarage)
                    {
                        wagonData.idTrainCar = 0;
                        wagonData.idParent = parent.net.ID.Value;
                    }
                    else
                    {
                        wagonData.idTrainCar = trainCar.net.ID.Value;
                        wagonData.idParent = 0;
                    }

                    wagonData.pos = posInGarage.Key.ToString();
                    wagonData.rot = posInGarage.Value.eulerAngles.ToString();

                    return wagonData;
                }

                internal void RemoveWagon()
                {
                    if (
                        owner != null
                        && ownIns.amountWagons.TryGetValue(owner, out int value)
                        && value > 0
                    )
                    {
                        ownIns.amountWagons[owner] = --value;
                    }

                    garage?.TryRemoveWagon(this);
                    DestroyModification();

                    // Удаляем блокировки с фундаментов
                    foreach (BuildingBlock buildingBlock in allBuilidngBlocks.ToHashSet())
                    {
                        if (ownIns.allFoundationsUnderWagons.ContainsKey(buildingBlock.net.ID.Value))
                        {
                            _ = ownIns.allFoundationsUnderWagons.Remove(buildingBlock.net.ID.Value);
                        }

                        buildingBlock.Kill();
                    }

                    if (parent != null)
                    {
                        stand.Destroy();
                        stand = null;
                        if (ownIns.allParents.ContainsKey(parent.net.ID.Value))
                        {
                            _ = ownIns.allParents.Remove(parent.net.ID.Value);
                        }

                        parent.Kill();
                    }

                    if (trainCar != null)
                    {
                        if (ownIns.allTrainCars.ContainsKey(trainCar.net.ID.Value))
                        {
                            _ = ownIns.allTrainCars.Remove(trainCar.net.ID.Value);
                        }

                        trainCar.Kill();
                    }

                    allNotBuilidngBlocks.Clear();
                    allBuilidngBlocks.Clear();
                    if (ownIns.wagons.Contains(this))
                    {
                        _ = ownIns.wagons.Remove(this);
                    }
                }

                internal void DestroyModification()
                {
                    if (modifications == null)
                    {
                        return;
                    }

                    modifications.RemoveModifications();
                    modifications.Kill();
                }

                internal void UpdateStateOnMonument(bool isBuildingState)
                {
                    onCustomMonument = isBuildingState;

                    if (isBuildingState)
                    {
                        UnParentIOEntity();
                    }
                    else
                    {
                        UpdateForNotBuildingBlocks();
                    }

                    SetParentForBuildingBlocks(GetActiveWagon);
                }

                internal HashSet<BaseEntity> GetAllDeployedObjects()
                {
                    return allNotBuilidngBlocks.Keys.ToHashSet();
                }

                internal void ChangeStateToLocked(BasePlayer player)
                {
                    ControllerSpawnsFreeWagon.RemoveFreeWagon(trainCar);
                    freeWagon = false;
                    owner = player;
                }

                #region House

                private void SpawnFloorsOnWagon()
                {
                    BaseEntity activeWagon = GetActiveWagon;
                    foreach (string localPos in localPosFloorOnWagon)
                    {
                        Vector3 newLocalPos =
                            activeWagon == parent
                                ? localPos.ToVector3()
                                : localPos.ToVector3() + new Vector3(0, 1.332f, 0);
                        Vector3 pos = GetGlobalPos(activeWagon.transform, newLocalPos);
                        Quaternion rot = GetGlobalRot(activeWagon.transform, new Vector3(0f, 0f, 0f));

                        BuildingBlock floor =
                            GameManager.server.CreateEntity(
                                "assets/prefabs/building core/floor/floor.prefab",
                                pos,
                                rot
                            ) as BuildingBlock;

                        floor!.enableSaving = true;
                        floor.transform.localPosition = newLocalPos;
                        floor.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
                        floor.SetParent(activeWagon);

                        floor.Spawn();

                        floor.lifestate = BaseCombatEntity.LifeState.Dead;
                        floor.AttachToBuilding(newBuildingID);
                        floor.SetHealthToMax();
                        floor.ChangeGradeAndSkin(BuildingGrade.Enum.Twigs, 0, true);

                        _ = allBuilidngBlocks.Add(floor);
                        ownIns.allBaseEntityByWagons.Add(floor.net.ID.Value, this);
                    }
                }

                internal void CheckNewEntity(
                    BaseEntity baseEntity,
                    BaseEntity target,
                    BasePlayer player
                )
                {
                    if (IsCupboard(baseEntity, player))
                    {
                        return;
                    }

                    TryDestroyGrounded(baseEntity);

                    TryMakeEntityNotDecay(baseEntity);

                    if (baseEntity is BuildingBlock)
                    {
                        CheckNewBuildingBlock(baseEntity, player);
                        return;
                    }

                    SetParentForNotBuildingBlock(baseEntity, target);

                    if (
                        IsEntityOnRoof(baseEntity.transform.position)
                        && !IsAllowPutOnRoof(baseEntity, player)
                    )
                    {
                        return;
                    }

                    if (
                        ownIns.acceptableEntities.ContainsKey(baseEntity.ShortPrefabName)
                        && !IsValidSettingsEntity(baseEntity, player)
                    )
                    {
                        return;
                    }

                    if (skinIdFromCustomEntity.Contains(baseEntity.skinID))
                    {
                        ProcessCustomEntity(baseEntity, target);
                    }

                    if (baseEntity is WeaponRack rack)
                    {
                        ProcessWeaponRack(rack);
                    }

                    allNotBuilidngBlocks.Add(baseEntity, target);

                    ownIns.allBaseEntityByWagons.Add(baseEntity.net.ID.Value, this);
                }

                private bool IsCupboard(BaseEntity baseEntity, BasePlayer player)
                {
                    if (baseEntity is BuildingPrivlidge)
                    {
                        NotifyManager.SendMessageToPlayer(
                            player,
                            "CanntPutEntity",
                            ownIns._config.NotifyConfig.prefix
                        );
                        CancelAction(baseEntity, player);
                        return true;
                    }

                    return false;
                }

                private void TryDestroyGrounded(BaseEntity baseEntity)
                {
                    GroundWatch groundWatch = baseEntity.GetComponent<GroundWatch>();
                    if (groundWatch != null)
                    {
                        UnityEngine.Object.DestroyImmediate(groundWatch);
                    }

                    DestroyOnGroundMissing destroyOnGroundMissing =
                        baseEntity.GetComponent<DestroyOnGroundMissing>();
                    if (destroyOnGroundMissing != null)
                    {
                        UnityEngine.Object.DestroyImmediate(destroyOnGroundMissing);
                    }

                    if (baseEntity is StabilityEntity stabilityEntity)
                    {
                        stabilityEntity.grounded = true;
                    }
                }

                private void TryMakeEntityNotDecay(BaseEntity entity)
                {
                    DecayEntity decay = entity as DecayEntity;
                    if (decay == null)
                    {
                        return;
                    }

                    decay.AttachToBuilding(newBuildingID);
                    decay.lastDecayTick = float.MaxValue;
                }

                private void CheckNewBuildingBlock(BaseEntity baseEntity, BasePlayer player)
                {
                    if (IsAllowPlaceBuildingBlock(baseEntity, player))
                    {
                        _ = allBuilidngBlocks.Add(baseEntity as BuildingBlock);
                        ownIns.allBaseEntityByWagons.Add(baseEntity.net.ID.Value, this);

                        SetParentForNewEntity(baseEntity);
                    }
                    else
                    {
                        CancelAction(baseEntity, player);
                    }
                }

                private bool IsAllowPutOnRoof(BaseEntity baseEntity, BasePlayer player)
                {
                    if (!ownIns._config.IsAllowPutOnRoof)
                    {
                        NotifyManager.SendMessageToPlayer(
                            player,
                            "CanntPutEntityOnRoof",
                            ownIns._config.NotifyConfig.prefix
                        );
                        CancelAction(baseEntity, player);
                        return false;
                    }

                    return true;
                }

                private bool IsValidSettingsEntity(BaseEntity baseEntity, BasePlayer player)
                {
                    SettingsEntity settingsEntity = ownIns.acceptableEntities[
                        baseEntity.ShortPrefabName
                    ];
                    if (!settingsEntity.IsAllowPut)
                    {
                        NotifyManager.SendMessageToPlayer(
                            player,
                            "CanntPutEntity",
                            ownIns._config.NotifyConfig.prefix
                        );
                        CancelAction(baseEntity, player);
                        return false;
                    }

                    int amount = allNotBuilidngBlocks.GetCountVariable(x =>
                        x.Key.ShortPrefabName == baseEntity.ShortPrefabName
                    );

                    if (settingsEntity.Amount <= amount)
                    {
                        NotifyManager.SendMessageToPlayer(
                            player,
                            "SetMaxAmountObjects",
                            ownIns._config.NotifyConfig.prefix
                        );
                        CancelAction(baseEntity, player);
                        return false;
                    }

                    return true;
                }

                private void ProcessCustomEntity(BaseEntity baseEntity, BaseEntity target)
                {
                    baseEntity.SetParent(target);
                }

                private void ProcessWeaponRack(WeaponRack rack)
                {
                    foreach (BaseEntity child in rack.children)
                    {
                        allNotBuilidngBlocks.Add(child, rack);
                        ownIns.allBaseEntityByWagons.Add(child.net.ID.Value, this);
                    }
                }

                private void SetParentForNewEntity(BaseEntity baseEntity)
                {
                    BaseEntity activeWagon = GetActiveWagon;
                    Vector3 localPos = activeWagon.transform.InverseTransformPoint(
                        baseEntity.transform.position
                    );
                    Vector3 rot = baseEntity.transform.rotation.eulerAngles;

                    baseEntity.SetParent(activeWagon);
                    baseEntity.transform.localPosition = localPos;

                    baseEntity.transform.rotation = Quaternion.Euler(rot);
                }

                internal void RemoveEntityInLists(BaseEntity entity)
                {
                    if (entity == null)
                    {
                        return;
                    }

                    if (entity is BuildingBlock block)
                    {
                        _ = allBuilidngBlocks.Remove(block);
                    }
                    else
                    {
                        _ = allNotBuilidngBlocks.Remove(entity);
                    }
                }

                #endregion House

                #region IsAllowBaseEntity

                private bool IsEntityOnRoof(Vector3 buildingPos)
                {
                    BaseEntity wagon = GetActiveWagon;
                    float localFloorHeight = wagon == parent ? localFloorHeight1 : localFloorHeight2;

                    return buildingPos.y
                        > GetActiveWagon.transform.position.y + localFloorHeight + 3.02f;
                }

                #endregion IsAllowBaseEntity

                #region IsAllowBuildingBlockPos

                private bool IsAllowPlaceBuildingBlock(BaseEntity baseEntity, BasePlayer player)
                {
                    return baseEntity.PrefabName switch
                    {
                        "assets/prefabs/building core/wall/wall.prefab"
                        or "assets/prefabs/building core/wall.doorway/wall.doorway.prefab"
                        or "assets/prefabs/building core/wall.window/wall.window.prefab"
                        or "assets/prefabs/building core/wall.frame/wall.frame.prefab" =>
                            IsAllowWallPos(baseEntity.transform.position, player),
                        "assets/prefabs/building core/foundation.steps/foundation.steps.prefab"
                        or "assets/prefabs/building core/ramp/ramp.prefab"
                        or "assets/prefabs/building core/wall.half/wall.half.prefab"
                        or "assets/prefabs/building core/wall.low/wall.low.prefab"
                        or "assets/prefabs/building core/stairs.u/block.stair.ushape.prefab"
                        or "assets/prefabs/building core/stairs.l/block.stair.lshape.prefab"
                        or "assets/prefabs/building core/stairs.spiral/block.stair.spiral.prefab"
                        or "assets/prefabs/building core/stairs.spiral.triangle/block.stair.spiral.triangle.prefab" =>
                            IsAllowBuildingBlockPos(baseEntity.transform.position, player),
                        "assets/prefabs/building core/floor/floor.prefab"
                        or "assets/prefabs/building core/floor.frame/floor.frame.prefab" =>
                            IsAllowFloorPos(baseEntity.transform.position, player),
                        "assets/prefabs/building core/floor.triangle/floor.triangle.prefab"
                        or "assets/prefabs/building core/floor.triangle.frame/floor.triangle.frame.prefab" =>
                            IsAllowFloorPos(
                                baseEntity.transform.position + baseEntity.transform.forward,
                                player
                            ),
                        "assets/prefabs/building core/roof/roof.prefab"
                        or "assets/prefabs/building core/roof.triangle/roof.triangle.prefab" =>
                            IsAllowRoofPos(baseEntity.transform.position, player),
                        _ => true,
                    };
                }

                private bool IsAllowBuildingBlockPos(Vector3 buildingPos, BasePlayer player)
                {
                    BaseEntity wagon = GetActiveWagon;
                    float localFloorHeight = wagon == parent ? localFloorHeight1 : localFloorHeight2;
                    if (buildingPos.y > wagon.transform.position.y + localFloorHeight + 2.98f)
                    {
                        NotifyManager.SendMessageToPlayer(
                            player,
                            "LimitFloorsForHouse",
                            ownIns._config.NotifyConfig.prefix
                        );
                        return false;
                    }
                    return true;
                }

                private bool IsAllowWallPos(Vector3 buildingPos, BasePlayer player)
                {
                    BaseEntity wagon = GetActiveWagon;
                    float localFloorHeight = wagon == parent ? localFloorHeight1 : localFloorHeight2;
                    if (buildingPos.y > wagon.transform.position.y + localFloorHeight + 0.03f)
                    {
                        NotifyManager.SendMessageToPlayer(
                            player,
                            "LimitFloorsForHouse",
                            ownIns._config.NotifyConfig.prefix
                        );
                        return false;
                    }
                    return true;
                }

                private bool IsAllowFloorPos(Vector3 buildingPos, BasePlayer player)
                {
                    Vector3 localPos = GetActiveWagon.transform.InverseTransformPoint(buildingPos);

                    if (
                        localPos.x > 1.4f
                        || localPos.x < -1.4f
                        || localPos.z > 7.4f
                        || localPos.z < -7.4f
                    )
                    {
                        NotifyManager.SendMessageToPlayer(
                            player,
                            "LimitFloorsForHouse",
                            ownIns._config.NotifyConfig.prefix
                        );
                        return false;
                    }

                    return true;
                }

                private bool IsAllowRoofPos(Vector3 buildingPos, BasePlayer player)
                {
                    if (!IsAllowFloorPos(buildingPos, player))
                    {
                        return false;
                    }

                    if (!IsAllowWallPos(buildingPos, player))
                    {
                        return false;
                    }

                    return true;
                }

                #endregion IsAllowBuildingBlockPos
            }

            private sealed class ModificationsWagon : DoorCloser
            {
                private Wagon? wagon { get; set; }

                private CodeLock? codelock { get; set; }
                private RFReceiver? receiver { get; set; }

                private CustomRANDSwitch? customRANDSwitch { get; set; }
                private RANDSwitch? randSwitch { get; set; }

                internal int frequency => receiver.frequency;
                internal string code => codelock.code;

                private int countdown { get; set; } = 10;
                private bool limNet;
                private bool firstCall = true;

                private static readonly HashSet<string> strForLimNet = new()
            {
                "assets/prefabs/building core/wall.frame/wall.frame.prefab",
                "assets/prefabs/building core/floor.frame/floor.frame.prefab",
                "assets/prefabs/building core/floor.triangle.frame/floor.triangle.frame.prefab",
            };

                internal void Init(BasePlayer player, Wagon Wagon, WagonData data)
                {
                    wagon = Wagon;
                    InvokeRepeating(Updater, 0f, 1f);

                    SetCodelock();
                    SetReceiver();
                    SetRANDSwitch();

                    if (data != null)
                    {
                        UpdateWithData(data);
                    }

                    UpdateConnections();
                    SendMessageToPlayer(player);
                }

                internal void InitForFreeWagon(Wagon Wagon)
                {
                    wagon = Wagon;
                    InvokeRepeating(Updater, 0f, 1f);

                    SetFreeCodeLock();
                }

                internal void ChangeStateToLocked(BasePlayer player)
                {
                    wagon.ChangeStateToLocked(player);

                    SetReceiver();
                    SetRANDSwitch();
                    UpdateConnections();
                    SendMessageToPlayer(player);
                }

                private void UpdateConnections()
                {
                    receiver.UpdateFromInput(100, 0);

                    receiver.outputs[0].connectedTo.Set(customRANDSwitch);
                    customRANDSwitch.inputs[0].connectedTo.Set(receiver);
                }

                private void SendMessageToPlayer(BasePlayer player)
                {
                    NotifyManager.SendMessageToPlayer(
                        player,
                        "CodeOfTheWagon",
                        ownIns._config.NotifyConfig.prefix,
                        codelock.code
                    );
                    NotifyManager.SendMessageToPlayer(
                        player,
                        "FrequencyOfTheWagon",
                        ownIns._config.NotifyConfig.prefix,
                        receiver.frequency.ToString()
                    );
                }

                private void UpdateWithData(WagonData data)
                {
                    UpdateCodelockWithData(data);
                    UpdateReceiverWithData(data);
                }

                private void UpdateCodelockWithData(WagonData data)
                {
                    codelock.code = data.code;
                    foreach (ulong playerID in data.authPlayers)
                    {
                        if (!codelock.whitelistPlayers.Contains(playerID))
                        {
                            codelock.whitelistPlayers.Add(playerID);
                        }
                    }
                }

                private void UpdateReceiverWithData(WagonData data)
                {
                    RFManager.ChangeFrequency(
                        receiver.frequency,
                        data.frequency,
                        receiver,
                        isListener: true
                    );
                    receiver.frequency = data.frequency;
                    receiver.MarkDirty();
                    SendNetworkUpdate();
                }

                private void Updater()
                {
                    if (wagon.trainCar == null)
                    {
                        return;
                    }

                    if (IsStopping())
                    {
                        return;
                    }

                    countdown--;
                    if (countdown == 0)
                    {
                        limNet = true;
                    }

                    if (!firstCall)
                    {
                        firstCall = true;
                    }

                    foreach (BuildingBlock block in wagon.allBuilidngBlocks)
                    {
                        if (block == null)
                        {
                            return;
                        }

                        if (limNet && strForLimNet.Contains(block.PrefabName))
                        {
                            block.limitNetworking = true;
                            block.limitNetworking = false;
                            countdown = 10;
                        }
                        SendNewWrite(block);
                    }

                    if (limNet)
                    {
                        limNet = false;
                    }

                    foreach (BaseEntity entity in wagon.GetAllDeployedObjects())
                    {
                        entity.SendNetworkUpdate();
                    }
                }

                private bool IsStopping()
                {
                    if (wagon.trainCar.IsStationary())
                    {
                        if (firstCall)
                        {
                            LimitNetworkingForBlocks();
                            firstCall = false;
                        }
                        return true;
                    }
                    return false;
                }

                internal void LimitNetworkingForBlocks()
                {
                    foreach (BuildingBlock block in wagon.allBuilidngBlocks)
                    {
                        if (block == null)
                        {
                            return;
                        }

                        block.limitNetworking = true;
                        block.limitNetworking = false;
                    }
                }

                private void SendNewWrite(BuildingBlock block)
                {
                    NetWrite newWrite = Net.sv.StartWrite();

                    newWrite.PacketID(Message.Type.RPCMessage);
                    newWrite.EntityID(block.net.ID);
                    newWrite.UInt32(StringPool.Get("RefreshSkin"));
                    newWrite.UInt64(0);
                    newWrite.Send(new SendInfo(block.net.group.subscribers));
                }

                private void SetCodelock()
                {
                    CreateCodelock();

                    _ = ownIns.allCodeLocks.Add(codelock);
                    codelock.SetParent(this, true, true);
                    codelock.transform.rotation = transform.rotation;
                    codelock.code = GetRandomCode();
                    codelock.hasCode = true;
                    codelock.SetFlag(Flags.Locked, true);
                    codelock.whitelistPlayers.Add(wagon.owner.userID);
                    codelock.enableSaving = false;
                    codelock.Spawn();
                }

                private void SetFreeCodeLock()
                {
                    CreateCodelock();

                    _ = ownIns.allCodeLocks.Add(codelock);
                    codelock.SetParent(this, true, true);
                    codelock.transform.rotation = transform.rotation;
                    codelock.SetFlag(Flags.Locked, false);
                    codelock.Spawn();
                }

                private void CreateCodelock()
                {
                    Vector3 pos = GetGlobalPos(transform, new Vector3(1.382f, 0.0f, 0f));
                    codelock =
                        GameManager.server.CreateEntity(
                            "assets/prefabs/locks/keypad/lock.code.prefab",
                            pos,
                            Quaternion.Euler(new Vector3(0f, 0f, 0))
                        ) as CodeLock;
                }

                private string GetRandomCode()
                {
                    string randomNumber = "";

                    for (int i = 0; i < 4; i++)
                    {
                        randomNumber += UnityEngine.Random.Range(0, 10).ToString();
                    }

                    return randomNumber;
                }

                private void SetReceiver()
                {
                    CreateReceiver();
                    Vector3 rot = receiver.transform.rotation.eulerAngles;

                    _ = ownIns.allReceivers.Add(receiver);
                    receiver.SetParent(this, true, true);

                    receiver.pickup.enabled = false;
                    receiver.transform.rotation = Quaternion.Euler(rot);
                    receiver.frequency = Convert.ToInt32(codelock.code);

                    receiver.enableSaving = false;
                    receiver.Spawn();
                }

                private void CreateReceiver()
                {
                    Vector3 pos = GetGlobalPos(transform, new Vector3(0.691f, 0f, 0.994f));
                    Quaternion rot = GetGlobalRot(transform, new Vector3(90f, 90f, 0f));

                    receiver =
                        GameManager.server.CreateEntity(
                            "assets/prefabs/deployable/playerioents/gates/rfreceiver/rfreceiver.prefab",
                            pos,
                            rot
                        ) as RFReceiver;
                }

                private void SetRANDSwitch()
                {
                    CreateRANDSwitch();

                    customRANDSwitch = randSwitch.gameObject.AddComponent<CustomRANDSwitch>();

                    CopySerializableFields(randSwitch, customRANDSwitch);
                    DestroyImmediate(randSwitch, true);

                    Vector3 rot = customRANDSwitch.transform.rotation.eulerAngles;
                    _ = ownIns.allSwitches.Add(customRANDSwitch);
                    customRANDSwitch.SetParent(this, true, true);
                    customRANDSwitch.transform.rotation = Quaternion.Euler(rot);

                    customRANDSwitch.wagon = wagon;
                    customRANDSwitch.pickup.enabled = false;

                    customRANDSwitch.enableSaving = false;
                    customRANDSwitch.Spawn();
                }

                private void CreateRANDSwitch()
                {
                    Vector3 pos = GetGlobalPos(transform, new Vector3(0f, 0f, 0f));
                    Quaternion rot = GetGlobalRot(transform, new Vector3(90f, 180f, 0f));

                    randSwitch =
                        GameManager.server.CreateEntity(
                            "assets/prefabs/deployable/playerioents/gates/randswitch/electrical.random.switch.deployed.prefab",
                            pos,
                            rot
                        ) as RANDSwitch;
                }

                internal void RemoveModifications()
                {
                    CancelInvoke(Updater);

                    _ = ownIns.allSwitches.Remove(customRANDSwitch);
                    _ = ownIns.allReceivers.Remove(receiver);
                    _ = ownIns.allCodeLocks.Remove(codelock);

                    codelock.Kill();
                    if (!wagon.freeWagon)
                    {
                        customRANDSwitch.Kill();
                        RFManager.RemoveListener(receiver.frequency, receiver);
                        receiver.Kill();
                    }
                }

                internal HashSet<ulong> GetWhitelist()
                {
                    return codelock.whitelistPlayers.ToHashSet();
                }
            }

            private sealed class CustomRANDSwitch : RANDSwitch
            {
                internal Wagon? wagon { get; set; }

                private ModificationsWagon? modifications { get; set; }
                private BasePlayer? caller;

                private bool firstCall = true;
                private bool falseCall;

                public override void UpdateFromInput(int inputAmount, int inputSlot)
                {
                    if (firstCall)
                    {
                        modifications = GetParentEntity() as ModificationsWagon;
                        firstCall = false;
                        return;
                    }

                    if (falseCall)
                    {
                        falseCall = false;
                        return;
                    }

                    falseCall = true;

                    if (inputAmount > 0)
                    {
                        if (wagon.onCustomMonument)
                        {
                            NotifyManager.SendMessageToPlayer(
                                caller,
                                "WagonOnMonument",
                                ownIns._config.NotifyConfig.prefix
                            );
                            return;
                        }

                        if (wagon.inGarage)
                        {
                            TrySpawnTrainCar();
                        }
                        else
                        {
                            TrySpawnBaseWagon();
                        }

                        caller = null;
                    }
                }

                private void TrySpawnTrainCar()
                {
                    caller = GetCallerInWhitelist();

                    if (caller != null)
                    {
                        if (ownIns.callers.Contains(caller))
                        {
                            NotifyManager.SendMessageToPlayer(
                                caller,
                                "CantCallAnotherWagon",
                                ownIns._config.NotifyConfig.prefix
                            );
                            return;
                        }
                        if (IsLongDistance())
                        {
                            return;
                        }

                        if (Interface.CallHook("OnWagonSpawn", caller) is bool)
                        {
                            return;
                        }

                        _ = ownIns.callers.Add(caller);
                        wagon.ThrowAwayFlare(caller);
                    }
                }

                private void TrySpawnBaseWagon()
                {
                    caller = GetCallerInWhitelist();

                    if (caller == null)
                    {
                        return;
                    }

                    if (ownIns.callers.Contains(caller))
                    {
                        NotifyManager.SendMessageToPlayer(
                            caller,
                            "CantCallAnotherWagon",
                            ownIns._config.NotifyConfig.prefix
                        );
                        return;
                    }
                    if (IsLongDistance())
                    {
                        return;
                    }

                    if (Interface.CallHook("OnBaseWagonSpawn", caller) is bool)
                    {
                        return;
                    }

                    _ = ownIns.callers.Add(caller);
                    wagon.ThrowAwayFlare(caller);
                }

                private BasePlayer? GetCallerInWhitelist()
                {
                    HashSet<BasePlayer> callers = new();

                    foreach (ulong steamID in modifications.GetWhitelist())
                    {
                        BasePlayer possibleCaller = BasePlayer.FindByID(steamID);
                        if (possibleCaller?.IsSleeping() != false)
                        {
                            continue;
                        }

                        HeldEntity heldEntity = possibleCaller.GetHeldEntity();
                        if (
                            heldEntity is not Detonator detonator
                            || detonator.frequency != modifications.frequency
                        )
                        {
                            continue;
                        }

                        caller = possibleCaller;
                        _ = callers.Add(caller);
                    }

                    if (callers.Count > 1)
                    {
                        foreach (BasePlayer Caller in callers)
                        {
                            NotifyManager.SendMessageToPlayer(
                                Caller,
                                "TwoCallers",
                                ownIns._config.NotifyConfig.prefix
                            );
                        }
                        return null;
                    }

                    return caller;
                }

                private bool IsLongDistance()
                {
                    float distance = Vector3.Distance(
                        wagon.parent != null
                            ? wagon.parent.transform.position
                            : wagon.trainCar.transform.position,
                        caller.transform.position
                    );

                    if (distance <= ownIns._config.MaxDistance)
                    {
                        return false;
                    }

                    NotifyManager.SendMessageToPlayer(
                        caller,
                        "LongDistance",
                        ownIns._config.NotifyConfig.prefix,
                        distance,
                        (float)ownIns._config.MaxDistance
                    );
                    return true;
                }
            }

            private sealed class Stand
            {
                public Wagon wagon { get; }
                private BaseEntity parent { get; }

                private BaseCombatEntity baseWagon { get; set; }
                private HashSet<BaseBarricade> allObjectOfStand { get; } = new();

                private static Dictionary<string, string> localPosObjects { get; } =
                    new()
                    {
                        ["(-2.214, 1.006, 6.944)"] = "(0, 183, 180)",
                        ["(0, 1.145, 8.298)"] = "(0, 356, 180)",
                        ["(2.355, 1.006, 6.944)"] = "(0, 340, 180)",
                        ["(2.286, 1.006, -6.648)"] = "(0, 15, 180)",
                        ["(0, 1.145, -8.348)"] = "(0, 6, 180)",
                        ["(-2.316, 1.006, -6.785)"] = "(0, 160, 180)",
                    };

                public Stand(BaseEntity Parent, Wagon Wagon)
                {
                    wagon = Wagon;
                    parent = Parent;
                    SpawnWagon();
                    SpawnStand();
                }

                public void Destroy()
                {
                    RemoveStand();
                    allObjectOfStand.Clear();
                }

                private void SpawnWagon()
                {
                    baseWagon = CreateBaseWagon.Start(
                        parent.transform.position,
                        parent.transform.rotation
                    );
                    baseWagon.Spawn();
                    ownIns.allStands.Add(baseWagon.net.ID.Value, this);
                }

                private void SpawnStand()
                {
                    foreach (KeyValuePair<string, string> kvp in localPosObjects)
                    {
                        Vector3 pos = GetGlobalPos(baseWagon.transform, kvp.Key.ToVector3());
                        Quaternion rot = GetGlobalRot(baseWagon.transform, kvp.Value.ToVector3());

                        BaseBarricade baseBarricade = CreateStandObject(pos, rot);

                        baseBarricade.SetParent(baseWagon, true, true);
                        baseBarricade.Spawn();

                        _ = allObjectOfStand.Add(baseBarricade);
                        ownIns.allStands.Add(baseBarricade.net.ID.Value, this);
                    }
                }

                private static BaseBarricade CreateStandObject(Vector3 pos, Quaternion rot)
                {
                    Barricade barricade =
                        GameManager.server.CreateEntity(
                            "assets/prefabs/deployable/barricades/barricade.stone.prefab",
                            pos,
                            rot
                        ) as Barricade;

                    BaseBarricade baseBarricade = barricade!.gameObject.AddComponent<BaseBarricade>();

                    CopySerializableFields((BaseCombatEntity)barricade, baseBarricade);

                    UnityEngine.Object.DestroyImmediate(barricade, true);

                    baseBarricade.enableSaving = true;

                    baseBarricade._maxHealth = ownIns._config.HpBarricade;
                    baseBarricade.startHealth = ownIns._config.HpBarricade;
                    baseBarricade.health = ownIns._config.HpBarricade;

                    return baseBarricade;
                }

                private void RemoveStand()
                {
                    foreach (BaseBarricade baseBarricade in allObjectOfStand)
                    {
                        _ = ownIns.allStands.Remove(baseBarricade.net.ID.Value);
                        baseBarricade.Kill();
                    }

                    if (baseWagon != null)
                    {
                        _ = ownIns.allStands.Remove(baseWagon.net.ID.Value);
                        baseWagon.Kill();
                    }
                }

                private static class CreateBaseWagon
                {
                    private static TrainCar? trainCar { get; set; }
                    private static BaseCombatEntity? baseWagon { get; set; }

                    internal static BaseCombatEntity Start(Vector3 pos, Quaternion rot)
                    {
                        trainCar =
                            GameManager.server.CreateEntity(
                                "assets/content/vehicles/trains/wagons/trainwagonc.entity.prefab",
                                new Vector3(pos.x, pos.y - 1.332f, pos.z),
                                rot
                            ) as TrainCar;

                        RemoveTriggers();

                        baseWagon = trainCar!.gameObject.AddComponent<BaseCombatEntity>();

                        CopySerializableFields(trainCar, baseWagon);

                        UnityEngine.Object.DestroyImmediate(trainCar, true);

                        baseWagon.enableSaving = false;

                        RemoveComponents();

                        baseWagon._maxHealth = ownIns._config.HpWagon;
                        baseWagon.startHealth = ownIns._config.HpWagon;
                        baseWagon.health = ownIns._config.HpWagon;

                        return baseWagon;
                    }

                    private static void RemoveTriggers()
                    {
                        UnityEngine.Object.Destroy(trainCar.frontCollisionTrigger);
                        UnityEngine.Object.Destroy(trainCar.rearCollisionTrigger);
                        UnityEngine.Object.Destroy(trainCar.hurtTriggerFront);
                        UnityEngine.Object.Destroy(trainCar.hurtTriggerRear);
                        UnityEngine.Object.Destroy(trainCar.platformParentTrigger);
                    }

                    private static void RemoveComponents()
                    {
                        Spawnable spawnable = baseWagon.GetComponent<Spawnable>();
                        if (spawnable != null)
                        {
                            UnityEngine.Object.Destroy(spawnable);
                        }

                        Model model = baseWagon.GetComponent<Model>();
                        if (model != null)
                        {
                            UnityEngine.Object.Destroy(model);
                        }

                        PrefabParameters prefabParameters = baseWagon.GetComponent<PrefabParameters>();
                        if (prefabParameters != null)
                        {
                            UnityEngine.Object.Destroy(prefabParameters);
                        }

                        Rigidbody rigidbody = baseWagon.GetComponent<Rigidbody>();
                        if (rigidbody != null)
                        {
                            UnityEngine.Object.Destroy(rigidbody);
                        }
                    }
                }
            }

            private sealed class SpawnerWagon : FacepunchBehaviour
            {
                private delegate void MyDelegate();

                private MyDelegate? methodBeingCalled;

                private const int validTopologies = (int)(
                    TerrainTopology.Enum.Rail | TerrainTopology.Enum.Railside
                );

                private Wagon? wagon { get; set; }

                private BuildingPrivlidge? cupboard;
                private BasePlayer? caller { get; set; }
                private BaseEntity? entityCollision { get; set; }
                private BaseEntity? thisEntity { get; set; }

                private bool active;

                private int countdown = 3;

                internal void Init(Wagon Wagon, BasePlayer player)
                {
                    wagon = Wagon;
                    caller = player;
                }

                private void OnCollisionEnter(Collision collision)
                {
                    if (active)
                    {
                        return;
                    }

                    thisEntity = gameObject.GetComponent<BaseEntity>();
                    active = true;

                    if (IsCollisionInBase(collision))
                    {
                        ownIns.NextTick(TryMoveWagonToBase);
                    }
                    else
                    {
                        TryMoveWagonOnTracks();
                    }
                }

                private bool IsCollisionInBase(Collision collision)
                {
                    entityCollision = collision.GetEntity();
                    if (entityCollision == null)
                    {
                        return false;
                    }

                    return IsTargetValid(thisEntity, entityCollision, caller);
                }

                private void TryMoveWagonToBase()
                {
                    if (
                        !ownIns._config.IsAllowEditWagonOnBase
                        || !IsValidVariablesForBase(caller, entityCollision, out cupboard)
                    )
                    {
                        CancelSpawn();
                        return;
                    }
                    if (IsWagonAlreadyBeingCalled())
                    {
                        return;
                    }

                    if (IsWagonInGarage())
                    {
                        return;
                    }

                    methodBeingCalled = SpawnInBase;
                    InvokeRepeating(Timer, 0f, 1f);
                }

                private void TryMoveWagonOnTracks()
                {
                    if (ownIns._config.CheckTopology && !IsValidTopologies())
                    {
                        return;
                    }

                    if (IsWagonAlreadyBeingCalled())
                    {
                        return;
                    }

                    if (IsWagonOnTrack())
                    {
                        return;
                    }

                    methodBeingCalled = SpawnOnTrack;
                    InvokeRepeating(Timer, 0f, 1f);
                }

                private bool IsValidTopologies()
                {
                    int currentTopologies = TerrainMeta.TopologyMap.GetTopology(
                        gameObject.transform.position
                    );
                    if ((currentTopologies & validTopologies) == 0)
                    {
                        NotifyManager.SendMessageToPlayer(
                            caller,
                            "NotValidTopologies",
                            ownIns._config.NotifyConfig.prefix
                        );
                        CancelSpawn();
                        return false;
                    }
                    return true;
                }

                private bool IsWagonAlreadyBeingCalled()
                {
                    if (ownIns.wagonsBeingCalled.Add(wagon))
                    {
                        return false;
                    }

                    NotifyManager.SendMessageToPlayer(
                        caller,
                        "WagonAlreadyCalled",
                        ownIns._config.NotifyConfig.prefix
                    );
                    thisEntity.Kill();
                    return true;
                }

                private bool IsWagonInGarage()
                {
                    if (wagon.inGarage)
                    {
                        NotifyManager.SendMessageToPlayer(
                            caller,
                            "WagonAlreadyInGarage",
                            ownIns._config.NotifyConfig.prefix
                        );
                        CancelSpawn();
                        return true;
                    }
                    return false;
                }

                private bool IsWagonOnTrack()
                {
                    if (!wagon.inGarage)
                    {
                        NotifyManager.SendMessageToPlayer(
                            caller,
                            "WagonAlreadyOnTracks",
                            ownIns._config.NotifyConfig.prefix
                        );
                        CancelSpawn();
                        return true;
                    }
                    return false;
                }

                private void Timer()
                {
                    if (countdown > 0)
                    {
                        NotifyManager.SendMessageToPlayer(
                            caller,
                            "TimerForSpawn",
                            ownIns._config.NotifyConfig.prefix,
                            countdown
                        );
                        countdown--;
                        return;
                    }
                    methodBeingCalled();
                    CancelSpawn();
                    CancelInvoke(Timer);
                }

                private void SpawnInBase()
                {
                    ownIns.TryCreateNewPosInGarage(entityCollision, caller, cupboard, wagon);
                }

                private void SpawnOnTrack()
                {
                    wagon.MoveWagonToTrack(gameObject.transform.position, caller);
                }

                private void CancelSpawn()
                {
                    thisEntity.Kill();
                    _ = ownIns.wagonsBeingCalled.Remove(wagon);
                    _ = ownIns.callers.Remove(caller);
                }
            }

            internal sealed class Garage
            {
                private Dictionary<
                    KeyValuePair<Vector3, Quaternion>,
                    HashSet<BuildingBlock>
                > placesForWagons
                { get; } = new();
                internal Dictionary<Vector3, Wagon> wagons { get; } = new();

                internal void InitWithData(GarageData garageData)
                {
                    foreach (PosInGarageData posData in garageData.poss)
                    {
                        if (!ownIns.doorCloserToWagon.ContainsKey(posData.idParent))
                        {
                            continue;
                        }

                        HashSet<BuildingBlock> foundations = new();
                        bool isValidPos = true;
                        CheckAllFoundations(posData, foundations, ref isValidPos);

                        if (isValidPos)
                        {
                            AddInGlobalHashSet(foundations);
                        }
                        else
                        {
                            continue;
                        }

                        KeyValuePair<Vector3, Quaternion> kvp = new(
                            posData.pos.ToVector3(),
                            Quaternion.Euler(posData.rot.ToVector3())
                        );

                        placesForWagons.Add(kvp, foundations);
                        DetermineWagons(posData);
                    }
                }

                private void CheckAllFoundations(
                    PosInGarageData posData,
                    HashSet<BuildingBlock> foundations,
                    ref bool isValidPos
                )
                {
                    foreach (ulong idFoundation in posData.foundations)
                    {
                        BuildingBlock foundation =
                            BaseNetworkable.serverEntities.FirstOrDefault(
                                p => p.net.ID.Value == idFoundation
                            ) as BuildingBlock;
                        if (foundation == null)
                        {
                            isValidPos = false;
                            Wagon wagon = ownIns.doorCloserToWagon[posData.idParent];
                            wagon.RemoveWagon();
                            break;
                        }
                        _ = foundations.Add(foundation);
                    }
                }

                private void AddInGlobalHashSet(HashSet<BuildingBlock> foundations)
                {
                    foreach (BuildingBlock foundation in foundations)
                    {
                        ownIns.allFoundationsUnderWagons.Add(foundation.net.ID.Value, this);
                    }
                }

                private void DetermineWagons(PosInGarageData posData)
                {
                    if (!ownIns.doorCloserToWagon.TryGetValue(posData.idParent, out Wagon? wagon))
                    {
                        return;
                    }

                    wagons.Add(posData.pos.ToVector3(), wagon);
                    wagon.garage = this;
                }

                internal void AddNewPos(
                    KeyValuePair<Vector3, Quaternion> newPosForWagon,
                    HashSet<BuildingBlock> foundationsUnderWagon
                )
                {
                    _ = placesForWagons.TryAdd(newPosForWagon, foundationsUnderWagon);
                    foreach (BuildingBlock block in foundationsUnderWagon)
                    {
                        if (!ownIns.allFoundationsUnderWagons.ContainsKey(block.net.ID.Value))
                        {
                            ownIns.allFoundationsUnderWagons.Add(block.net.ID.Value, this);
                        }
                    }
                }

                internal void TryRemoveWagon(Wagon wagon)
                {
                    foreach (KeyValuePair<Vector3, Wagon> kvp in wagons.ToHashSet())
                    {
                        if (kvp.Value != wagon)
                        {
                            continue;
                        }

                        RemovePlace(kvp.Key);
                        _ = wagons.Remove(kvp.Key);
                    }
                }

                private void RemovePlace(Vector3 pos)
                {
                    KeyValuePair<KeyValuePair<Vector3, Quaternion>, HashSet<BuildingBlock>> kvp =
                        placesForWagons.FirstOrDefault(x => x.Key.Key == pos);

                    foreach (BuildingBlock block in kvp.Value)
                    {
                        _ = ownIns.allFoundationsUnderWagons.Remove(block.net.ID.Value);
                    }

                    _ = placesForWagons.Remove(kvp.Key);
                }

                internal void DestroyAllWagons()
                {
                    foreach (KeyValuePair<Vector3, Wagon> kvp in wagons.ToHashSet())
                    {
                        kvp.Value.RemoveWagon();
                    }
                }

                internal Vector3? GetPosWagon(BuildingBlock foundation)
                {
                    KeyValuePair<KeyValuePair<Vector3, Quaternion>, HashSet<BuildingBlock>> kvp =
                        placesForWagons.FirstOrDefault(x => x.Value.Contains(foundation));
                    if (
                        kvp.Equals(
                            default(KeyValuePair<
                                KeyValuePair<Vector3, Quaternion>,
                                HashSet<BuildingBlock>
                            >)
                        )
                    )
                    {
                        return null;
                    }

                    return kvp.Key.Key;
                }

                internal Wagon? GetWagon(Vector3 pos)
                {
                    KeyValuePair<Vector3, Wagon> kvp = wagons.FirstOrDefault(
                        x => x.Key == pos
                    );
                    if (kvp.Equals(default(KeyValuePair<Vector3, Wagon>)))
                    {
                        return null;
                    }

                    return kvp.Value;
                }

                internal GarageData CreateInfoForData(ulong cupboard)
                {
                    GarageData garageData = new() { idCupboard = cupboard };

                    foreach (
                        KeyValuePair<
                            KeyValuePair<Vector3, Quaternion>,
                            HashSet<BuildingBlock>
                        > kvp in placesForWagons
                    )
                    {
                        Vector3 posWagon = kvp.Key.Key;
                        Vector3 rotWagon = kvp.Key.Value.eulerAngles;
                        PosInGarageData posInGarageData = new()
                        {
                            pos = posWagon.ToString(),
                            rot = rotWagon.ToString(),
                        };

                        try
                        {
                            posInGarageData.idParent = wagons.TryGetValue(posWagon, out Wagon? wagon)
                                ? wagon.parent.net.ID.Value
                                : 0;
                        }
                        catch (Exception e)
                        {
                            DebugError(
                                $"------Error------\n{e}\n------Info------\nwagon == null\n----------------"
                            );
                        }

                        try
                        {
                            foreach (BuildingBlock foundation in kvp.Value)
                            {
                                _ = posInGarageData.foundations.Add(foundation.net.ID.Value);
                            }
                        }
                        catch (Exception e)
                        {
                            DebugError(
                                $"------Error------\n{e}\n------Info------\nfoundation == null\n----------------"
                            );
                        }

                        _ = garageData.poss.Add(posInGarageData);
                    }

                    return garageData;
                }
            }

            internal sealed class ConstructionMatrix
            {
                private BasePlayer? player { get; set; }
                private BuildingBlock? mainFoundation { get; set; }
                internal KeyValuePair<Vector3, Quaternion> posForSpawnWagon { get; private set; }
                internal bool isValid;

                private List<BuildingBlock>? blocksAroundMainFoundation;
                internal HashSet<BuildingBlock> blocksUnderWagon { get; } = new();

                private List<BuildingBlock>? blocks;
                private HashSet<string> poss { get; set; } = new();

                private static readonly HashSet<string> validLocalPosFoundations1 = new()
            {
                "(-3, 0, 0)",
                "(-6, 0, 0)",
                "(-9, 0, 0)",
                "(3, 0, 0)",
                "(6, 0, 0)",
                "(9, 0, 0)",
                "(0, 0, 3)",
                "(0, 0, -3)",
                "(3, 0, 3)",
                "(6, 0, 3)",
                "(9, 0, 3)",
                "(3, 0, -3)",
                "(6, 0, -3)",
                "(9, 0, -3)",
                "(-3, 0, 3)",
                "(-6, 0, 3)",
                "(-9, 0, 3)",
                "(-3, 0, -3)",
                "(-6, 0, -3)",
                "(-9, 0, -3)",
            };

                private static readonly HashSet<string> validLocalPosFoundations2 = new()
            {
                "(0, 0, 3)",
                "(0, 0, 6)",
                "(0, 0, 9)",
                "(0, 0, -3)",
                "(0, 0, -6)",
                "(0, 0, -9)",
                "(3, 0, 0)",
                "(-3, 0, 0)",
                "(-3, 0, 3)",
                "(-3, 0, 6)",
                "(-3, 0, 9)",
                "(-3, 0, -3)",
                "(-3, 0, -6)",
                "(-3, 0, -9)",
                "(3, 0, -3)",
                "(3, 0, -6)",
                "(3, 0, -9)",
                "(3, 0, 3)",
                "(3, 0, 6)",
                "(3, 0, 9)",
            };

                internal void Init(
                    BaseEntity baseEntity,
                    List<BuildingBlock> foundations,
                    BasePlayer Player
                )
                {
                    player = Player;

                    mainFoundation = baseEntity as BuildingBlock;
                    blocksAroundMainFoundation = foundations;

                    if (blocksAroundMainFoundation.Contains(mainFoundation))
                    {
                        _ = blocksAroundMainFoundation.Remove(mainFoundation);
                    }

                    StartCheck();

                    _ = blocksUnderWagon.Add(mainFoundation);

                    Pool.FreeUnmanaged(ref blocksAroundMainFoundation);
                    Pool.FreeUnmanaged(ref blocks);
                }

                private void StartCheck()
                {
                    if (IsValidConstruction(validLocalPosFoundations1))
                    {
                        isValid = true;
                        return;
                    }

                    blocksUnderWagon.Clear();
                    if (IsValidConstruction(validLocalPosFoundations2))
                    {
                        isValid = true;
                    }
                }

                private bool IsValidConstruction(HashSet<string> hashSet)
                {
                    UpdateVariables(hashSet);
                    CheckFoundations(poss);

                    if (
                        poss.Count == 0
                        && IsPosFoundationsValid()
                        && !IsExistFoundationsInGlobalHashSet()
                    )
                    {
                        Quaternion quaternion = GetQuaternionForSpawnWagon(hashSet);
                        posForSpawnWagon = new KeyValuePair<Vector3, Quaternion>(
                            mainFoundation.transform.position,
                            quaternion
                        );
                        return true;
                    }

                    return false;
                }

                private Quaternion GetQuaternionForSpawnWagon(HashSet<string> hashSet)
                {
                    Vector3 localPos = hashSet.ElementAt(3).ToVector3();
                    Vector3 pos1 = GetGlobalPos(mainFoundation.transform, localPos);

                    return Quaternion.LookRotation(pos1 - mainFoundation.transform.position);
                }

                private void UpdateVariables(HashSet<string> hashSet)
                {
                    blocks = blocksAroundMainFoundation.ToList();
                    poss = new HashSet<string>(hashSet);
                }

                private void CheckFoundations(HashSet<string> validLocalPosFoundationsCopy)
                {
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        BuildingBlock block = blocks[i];

                        Vector3 localPos = mainFoundation.transform.InverseTransformPoint(
                            block.transform.position
                        );

                        if (validLocalPosFoundationsCopy.Count == 0)
                        {
                            break;
                        }

                        foreach (string stringPos in validLocalPosFoundationsCopy.ToHashSet())
                        {
                            if (Vector3.Distance(stringPos.ToVector3(), localPos) < 0.2f)
                            {
                                _ = blocksUnderWagon.Add(block);
                                _ = blocks.Remove(block);

                                _ = validLocalPosFoundationsCopy.Remove(stringPos);

                                i--;
                                break;
                            }
                        }
                    }
                }

                private bool IsPosFoundationsValid()
                {
                    float posY1 = mainFoundation.transform.position.y;

                    foreach (BuildingBlock buildingBlock in blocksUnderWagon)
                    {
                        float posY2 = buildingBlock.transform.position.y;

                        if (
                            Vector3.Distance(new Vector3(0f, posY1, 0f), new Vector3(0f, posY2, 0f))
                            > 0.1f
                        )
                        {
                            return false;
                        }
                    }

                    return true;
                }

                private bool IsExistFoundationsInGlobalHashSet()
                {
                    foreach (BuildingBlock buildingBlock in blocksUnderWagon)
                    {
                        if (ownIns.allFoundationsUnderWagons.ContainsKey(buildingBlock.net.ID.Value))
                        {
                            NotifyManager.SendMessageToPlayer(
                                player,
                                "FoundationsExistInHashSet",
                                ownIns._config.NotifyConfig.prefix
                            );
                            return true;
                        }
                    }
                    return false;
                }
            }

            internal static class ControllerSpawnsFreeWagon
            {
                private static HashSet<TrainCar> trainCars = new();
                internal static readonly HashSet<TrainCar> freeWagons = new();

                private static readonly HashSet<string> strValidWagons = new()
            {
                "trainwagona.entity",
                "trainwagonb.entity",
                "trainwagonc.entity",
            };

                private static Coroutine? timer { get; set; }

                internal static void Init()
                {
                    if (timer != null)
                    {
                        return;
                    }

                    trainCars = BaseNetworkable
                        .serverEntities.Where(x =>
                            x is TrainCar trainCar
                            && strValidWagons.Contains(trainCar.ShortPrefabName)
                            && !ownIns.allTrainCars.ContainsKey(trainCar.net.ID.Value)
                        )
                        .Cast<TrainCar>()
                        .ToHashSet();

                    if (ownIns.plugins.Exists("ArmoredTrain"))
                    {
                        foreach (TrainCar? trainCar in trainCars.ToHashSet())
                        {
                            if (IsArmoredTrain(trainCar))
                            {
                                _ = trainCars.Remove(trainCar);
                            }
                        }
                    }

                    ownIns.onServerInit = true;

                    timer = ServerMgr.Instance.StartCoroutine(Timer());
                }

                internal static void Unload()
                {
                    if (timer != null)
                    {
                        ServerMgr.Instance.StopCoroutine(timer);
                    }

                    timer = null;

                    foreach (TrainCar trainCar in freeWagons.ToHashSet())
                    {
                        if (trainCar.IsExists())
                        {
                            trainCar.Kill();
                        }
                    }

                    freeWagons.Clear();
                    trainCars.Clear();
                }

                private static IEnumerator Timer()
                {
                    while (true)
                    {
                        if (freeWagons.Count < ownIns._config.amountFreeWagons && trainCars.Count > 0)
                        {
                            foreach (TrainCar trainCar in trainCars.ToHashSet())
                            {
                                if (freeWagons.Count == ownIns._config.amountFreeWagons)
                                {
                                    break;
                                }

                                if (
                                    trainCar == null
                                    || trainCar.net == null
                                    || ownIns.allTrainCars.ContainsKey(trainCar.net.ID.Value)
                                    || ownIns.allParents.ContainsKey(trainCar.net.ID.Value)
                                )
                                {
                                    _ = trainCars.Remove(trainCar);
                                    continue;
                                }

                                trainCar.Kill();
                                Wagon wagon = new();
                                wagon.InitFreeWagon(
                                    trainCar.transform.position,
                                    trainCar.transform.rotation
                                );
                                CheckTrainCar(wagon, trainCar);
                            }
                        }

                        ownIns.PrintWarning($"Count of free wagons: {freeWagons.Count}");
                        yield return CoroutineEx.waitForSeconds(750.0f);
                    }
                }

                private static void CheckTrainCar(Wagon wagon, TrainCar trainCar)
                {
                    if (wagon.trainCar == null)
                    {
                        wagon.RemoveWagon();
                        return;
                    }

                    _ = trainCars.Remove(trainCar);
                    _ = freeWagons.Add(wagon.trainCar);
                }

                internal static void RemoveFreeWagon(TrainCar trainCar)
                {
                    _ = freeWagons.Remove(trainCar);
                    _ = trainCars.Remove(trainCar);
                }

                internal static void TryAddNewTrainCar(TrainCar trainCar)
                {
                    if (trainCar == null || trainCar.net == null)
                    {
                        return;
                    }

                    if (ownIns.plugins.Exists("ArmoredTrain") && IsArmoredTrain(trainCar))
                    {
                        return;
                    }

                    if (IsValidTrainCar(trainCar))
                    {
                        _ = trainCars.Add(trainCar);
                    }
                }

                private static bool IsValidTrainCar(TrainCar trainCar)
                {
                    return ownIns.onServerInit
                        && strValidWagons.Contains(trainCar.ShortPrefabName)
                        && !freeWagons.Contains(trainCar);
                }

                private static bool IsArmoredTrain(TrainCar trainCar)
                {
                    return ownIns.ArmoredTrain.Call<bool>("IsTrainWagon", trainCar.net.ID.Value);
                }

                internal static void ShowPosFreeWagons(BasePlayer player)
                {
                    ownIns.Puts($"Count Free Wagons: {freeWagons.Count}");

                    foreach (TrainCar trainCar in freeWagons)
                    {
                        player.SendConsoleCommand(
                            "ddraw.line",
                            10f,
                            Color.green,
                            trainCar.transform.position,
                            trainCar.transform.position + (Vector3.up * 200f)
                        );
                    }
                }
            }

            internal sealed class CustomMonument : FacepunchBehaviour
            {
                private ElectricalBranch? branch { get; set; }
                private PressButton? button { get; set; }
                private AnimationTransformVehicle? animation { get; set; }
                private TrainCar? activeTrainCar { get; set; }

                private BoxCollider? collider { get; set; }

                internal bool isBuildingState;
                private const int delay = 200;
                private readonly Vector3 localPosForWagon = new(0f, 0.413f, 0f);

                private HashSet<BaseEntity> objectsOfWalls { get; } = new();

                private static readonly Dictionary<Vector3, Vector3> possBarricades = new()
                {
                    [new Vector3(-2.415f, 0f, 12.626f)] = new Vector3(0f, 332.401f, 0f),
                    [new Vector3(0f, 0f, 13.194f)] = new Vector3(0f, 0f, 0f),
                    [new Vector3(2.415f, 0f, 12.626f)] = new Vector3(0f, 27.6f, 0f),
                    [new Vector3(2.415f, 0f, -11.430f)] = new Vector3(0f, 152.401f, 0f),
                    [new Vector3(0f, 0f, -11.998f)] = new Vector3(0f, 180f, 0f),
                    [new Vector3(-2.415f, 0f, -11.430f)] = new Vector3(0f, 207.6f, 0f),
                };

                private static readonly Dictionary<Vector3, Vector3> possStairs = new()
                {
                    [new Vector3(2.988f, -0.376f, -11.142f)] = new Vector3(0f, 242.401f, 63.175f),
                    [new Vector3(1.852f, -0.376f, -11.737f)] = new Vector3(0f, 242.401f, 63.175f),
                    [new Vector3(0.641f, -0.376f, -12.009f)] = new Vector3(0f, 270f, 63.175f),
                    [new Vector3(-0.641f, -0.376f, -12.009f)] = new Vector3(0f, 270f, 63.175f),
                    [new Vector3(-2.988f, -0.376f, -11.142f)] = new Vector3(0f, 297.6f, 63.175f),
                    [new Vector3(-1.852f, -0.376f, -11.737f)] = new Vector3(0f, 297.6f, 63.175f),
                    [new Vector3(-2.988f, -0.376f, 12.339f)] = new Vector3(0f, 62.4f, 63.175f),
                    [new Vector3(-1.852f, -0.376f, 12.933f)] = new Vector3(0f, 62.4f, 63.175f),
                    [new Vector3(0.641f, -0.376f, 13.206f)] = new Vector3(0f, 90f, 63.175f),
                    [new Vector3(-0.641f, -0.376f, 13.205f)] = new Vector3(0f, 90f, 63.175f),
                    [new Vector3(2.988f, -0.376f, 12.339f)] = new Vector3(0f, 117.6f, 63.175f),
                    [new Vector3(1.852f, -0.376f, 12.932f)] = new Vector3(0f, 117.6f, 63.175f),
                };

                private static readonly Dictionary<Vector3, Vector3> possSigns = new()
                {
                    [new Vector3(-2.44f, 0.268f, 12.674f)] = new Vector3(0f, 152.4f, 0f),
                    [new Vector3(0f, 0.268f, 13.248f)] = new Vector3(0f, 180f, 0f),
                    [new Vector3(2.44f, 0.268f, 12.674f)] = new Vector3(0f, 207.6f, 0f),
                    [new Vector3(2.44f, 0.268f, -11.477f)] = new Vector3(0f, 332.4f, 0f),
                    [new Vector3(0f, 0.268f, -12.052f)] = new Vector3(0f, 0f, 0f),
                    [new Vector3(-2.44f, 0.268f, -11.477f)] = new Vector3(0f, 27.6f, 0f),
                };

                private static readonly Dictionary<Vector3, Vector3> possDoorBarricades = new()
                {
                    [new Vector3(2.4f, -0.738f, -11.4f)] = new Vector3(0f, 332.4f, 0f),
                    [new Vector3(0f, -0.738f, -11.965f)] = new Vector3(0f, 0f, 0f),
                    [new Vector3(-2.4f, -0.738f, -11.4f)] = new Vector3(0f, 27.6f, 0f),
                    [new Vector3(-2.4f, -0.738f, 12.597f)] = new Vector3(0f, 152.4f, 0f),
                    [new Vector3(0f, -0.738f, 13.162f)] = new Vector3(0f, 180f, 0f),
                    [new Vector3(2.4f, -0.738f, 12.597f)] = new Vector3(0f, 207.6f, 0f),
                };

                private static readonly Dictionary<Vector3, Vector3> possRoadSigns = new()
                {
                    [new Vector3(-2.46f, -2.013f, 12.712f)] = new Vector3(346.296f, 62.4f, 0f),
                    [new Vector3(-2.46f, -1.29f, 12.712f)] = new Vector3(0f, 62.4f, 0f),
                    [new Vector3(-2.46f, -2.013f, 12.713f)] = new Vector3(13.172f, 62.4f, 0f),
                    [new Vector3(0f, -2.013f, 13.291f)] = new Vector3(346.296f, 90f, 0f),
                    [new Vector3(0f, -1.29f, 13.291f)] = new Vector3(0f, 90f, 0f),
                    [new Vector3(0f, -2.013f, 13.292f)] = new Vector3(13.172f, 90f, 0f),
                    [new Vector3(2.46f, -2.013f, 12.712f)] = new Vector3(346.296f, 117.6f, 0f),
                    [new Vector3(2.46f, -1.29f, 12.712f)] = new Vector3(0f, 117.6f, 0f),
                    [new Vector3(2.46f, -2.013f, 12.713f)] = new Vector3(13.172f, 117.6f, 0f),
                    [new Vector3(2.46f, -2.013f, -11.515f)] = new Vector3(346.296f, 242.4f, 0f),
                    [new Vector3(2.46f, -1.29f, -11.515f)] = new Vector3(0f, 242.4f, 0f),
                    [new Vector3(2.46f, -2.013f, -11.516f)] = new Vector3(13.172f, 242.4f, 0f),
                    [new Vector3(0f, -2.013f, -12.095f)] = new Vector3(346.296f, 270f, 0f),
                    [new Vector3(0f, -1.29f, -12.095f)] = new Vector3(0f, 270f, 0f),
                    [new Vector3(0f, -2.013f, -12.096f)] = new Vector3(13.172f, 270f, 0f),
                    [new Vector3(-2.46f, -2.013f, -11.515f)] = new Vector3(346.296f, 297.6f, 0f),
                    [new Vector3(-2.46f, -1.29f, -11.515f)] = new Vector3(0f, 297.6f, 0f),
                    [new Vector3(-2.46f, -2.013f, -11.516f)] = new Vector3(13.172f, 297.6f, 0f),
                };

                private static readonly HashSet<string> strRoadSigns = new()
            {
                "assets/content/props/roadsigns/roadsign1.prefab",
                "assets/content/props/roadsigns/roadsign2.prefab",
                "assets/content/props/roadsigns/roadsign3.prefab",
                "assets/content/props/roadsigns/roadsign4.prefab",
                "assets/content/props/roadsigns/roadsign5.prefab",
                "assets/content/props/roadsigns/roadsign6.prefab",
                "assets/content/props/roadsigns/roadsign7.prefab",
                "assets/content/props/roadsigns/roadsign8.prefab",
                "assets/content/props/roadsigns/roadsign9.prefab",
            };

                private void OnTriggerEnter(Collider other)
                {
                    BasePlayer player = other.GetComponentInParent<BasePlayer>();
                    if (player.IsPlayer())
                    {
                        OnEnterPlayer(player);
                    }
                }

                private void OnEnterPlayer(BasePlayer player)
                {
                    if (!ownIns.playersOnMonuments.ContainsKey(player))
                    {
                        ownIns.playersOnMonuments.Add(player, this);
                    }
                }

                private void OnTriggerExit(Collider other)
                {
                    BasePlayer player = other.GetComponentInParent<BasePlayer>();
                    if (player.IsPlayer())
                    {
                        OnExitPlayer(player);
                    }
                }

                internal void OnExitPlayer(BasePlayer player)
                {
                    _ = ownIns.playersOnMonuments.Remove(player);
                }

                internal void Init(ElectricalBranch electricalBranch)
                {
                    Vector3 pos = electricalBranch.transform.position;
                    transform.position = new Vector3(pos.x, pos.y, pos.z + 0.6f);
                    transform.rotation = electricalBranch.transform.rotation;
                    InitCollider();

                    branch = electricalBranch;
                    CreateButton();
                }

                private void InitCollider()
                {
                    gameObject.layer = 3;
                    collider = gameObject.AddComponent<BoxCollider>();
                    collider.isTrigger = true;
                    collider.size = new Vector3(17f, 18.665f, 32f);
                }

                private void CreateButton()
                {
                    Vector3 pos = GetGlobalPos(branch.transform, new Vector3(3.411f, 0.057f, -4.668f));
                    Quaternion rot = GetGlobalRot(branch.transform, new Vector3(0f, 0f, 0f));

                    button =
                        GameManager.server.CreateEntity(
                            "assets/prefabs/io/electric/switches/pressbutton/pressbutton.prefab",
                            pos,
                            rot
                        ) as PressButton;

                    button!.enableSaving = false;
                    button.pickup.enabled = false;

                    button.Spawn();

                    ownIns.buttonsOnMonuments.Add(button, this);
                }

                internal void TryUpdateState(BasePlayer player)
                {
                    if (isBuildingState)
                    {
                        UpdateState(activeTrainCar);
                        return;
                    }

                    HashSet<TrainCar> trainCarsAround = GetTrainCarsAroundMonument();

                    if (!IsValidCountTrainCar(trainCarsAround, player))
                    {
                        return;
                    }

                    TrainCar trainCar = trainCarsAround.ElementAt(0);
                    if (!IsCustomWagon(trainCar, player))
                    {
                        return;
                    }

                    if (IsFreeWagon(trainCar, player))
                    {
                        return;
                    }

                    TryDisconnectOtherTrainCars(trainCar);
                    AddAnimationToTrainCar(trainCar, player);
                }

                private HashSet<TrainCar> GetTrainCarsAroundMonument()
                {
                    Vector3 pos = branch.transform.position;
                    return GetEntities<TrainCar>(new Vector3(pos.x, pos.y, pos.z + 0.6f), 14f);
                }

                private bool IsValidCountTrainCar(HashSet<TrainCar> trainCarsAround, BasePlayer player)
                {
                    if (trainCarsAround.Count == 0)
                    {
                        NotifyManager.SendMessageToPlayer(
                            player,
                            "NotHaveWagons",
                            ownIns._config.NotifyConfig.prefix
                        );
                        return false;
                    }

                    if (trainCarsAround.Count > 1)
                    {
                        NotifyManager.SendMessageToPlayer(
                            player,
                            "MuchWagons",
                            ownIns._config.NotifyConfig.prefix
                        );
                        return false;
                    }

                    return true;
                }

                private bool IsCustomWagon(TrainCar trainCar, BasePlayer player)
                {
                    if (!ownIns.allTrainCars.ContainsKey(trainCar.net.ID.Value))
                    {
                        NotifyManager.SendMessageToPlayer(
                            player,
                            "NotCustomWagon",
                            ownIns._config.NotifyConfig.prefix
                        );
                        return false;
                    }

                    return true;
                }

                private bool IsFreeWagon(TrainCar trainCar, BasePlayer player)
                {
                    Wagon wagon = ownIns.allTrainCars[trainCar.net.ID.Value];
                    if (wagon.freeWagon)
                    {
                        NotifyManager.SendMessageToPlayer(
                            player,
                            "CantBuildingOnFreeWagon",
                            ownIns._config.NotifyConfig.prefix
                        );
                        return true;
                    }

                    return false;
                }

                private void TryDisconnectOtherTrainCars(TrainCar trainCar)
                {
                    trainCar.coupling.frontCoupling.Uncouple(true);
                    trainCar.coupling.rearCoupling.Uncouple(true);
                }

                private void AddAnimationToTrainCar(TrainCar trainCar, BasePlayer player)
                {
                    animation = trainCar.gameObject.AddComponent<AnimationTransformVehicle>();

                    PointAnimationTransform point2 = new()
                    {
                        Pos = GetGlobalPos(branch.transform, localPosForWagon),
                        Rot = GetGlobalRot(branch.transform, new Vector3(0f, 0f, 0f)).eulerAngles,
                    };
                    animation.whoTryMoveWagon = player;
                    animation.customMonument = this;
                    animation.AddPath(point2, 5f);
                }

                internal void TryEnabledBuildingState(BasePlayer player)
                {
                    DestroyImmediate(animation);
                    HashSet<TrainCar> trainCarsAround = GetTrainCarsAroundMonument();
                    if (!IsValidCountTrainCar(trainCarsAround, player))
                    {
                        return;
                    }

                    UpdateState(trainCarsAround.ElementAt(0));
                }

                private void UpdateState(TrainCar trainCar)
                {
                    if (isBuildingState)
                    {
                        activeTrainCar = null;
                        isBuildingState = false;
                        DestroyWalls();
                    }
                    else
                    {
                        activeTrainCar = trainCar;
                        isBuildingState = true;
                        CreateWalls();
                    }

                    Wagon wagon = GetWagon(trainCar);
                    wagon.UpdateStateOnMonument(isBuildingState);
                }

                private void DestroyWalls()
                {
                    if (objectsOfWalls.Count > 0)
                    {
                        Effect.server.Run(
                            "assets/bundled/prefabs/fx/building/stone_gib.prefab",
                            GetGlobalPos(branch.transform, new Vector3(0f, 0f, 13.194f))
                        );
                        Effect.server.Run(
                            "assets/bundled/prefabs/fx/building/stone_gib.prefab",
                            GetGlobalPos(branch.transform, new Vector3(0f, 0f, -11.998f))
                        );
                    }

                    foreach (BaseEntity entity in objectsOfWalls.ToHashSet())
                    {
                        _ = ownIns.allObjectsOfWalls.Remove(entity.net.ID.Value);
                        _ = objectsOfWalls.Remove(entity);
                        entity.Kill();
                    }
                }

                private void CreateWalls()
                {
                    SortThroughPossOfObject(
                        possBarricades,
                        "assets/prefabs/deployable/barricades/barricade.concrete.prefab"
                    );
                    SortThroughPossOfObject(
                        possStairs,
                        "assets/prefabs/building core/foundation.steps/foundation.steps.prefab"
                    );
                    SortThroughPossOfObject(
                        possSigns,
                        "assets/prefabs/misc/xmas/neon_sign/sign.neon.xl.prefab"
                    );
                    SortThroughPossOfObject(
                        possDoorBarricades,
                        "assets/prefabs/deployable/door barricades/door_barricade_dbl_b.prefab"
                    );
                    SortThroughPossOfObject(possRoadSigns);
                }

                private void SpawnObject(string strPrefab, Vector3 pos, Quaternion rot)
                {
                    BaseEntity entity = GameManager.server.CreateEntity(strPrefab, pos, rot);

                    entity.enableSaving = false;

                    GroundWatch groundWatch = entity.GetComponent<GroundWatch>();
                    if (groundWatch != null)
                    {
                        DestroyImmediate(groundWatch);
                    }

                    DestroyOnGroundMissing destroyOnGroundMissing =
                        entity.GetComponent<DestroyOnGroundMissing>();
                    if (destroyOnGroundMissing != null)
                    {
                        DestroyImmediate(destroyOnGroundMissing);
                    }

                    entity.Spawn();

                    if (entity is BuildingBlock block)
                    {
                        block.ChangeGrade(BuildingGrade.Enum.Stone);
                        block.SetHealthToMax();
                    }
                    if (entity is StabilityEntity stabilityEntity)
                    {
                        stabilityEntity.grounded = true;
                    }

                    if (entity is BaseCombatEntity baseCombatEntity)
                    {
                        baseCombatEntity.pickup.enabled = false;
                    }

                    if (entity is NeonSign)
                    {
                        entity.SetFlag(BaseEntity.Flags.Busy, true);
                    }

                    _ = objectsOfWalls.Add(entity);
                    _ = ownIns.allObjectsOfWalls.Add(entity.net.ID.Value);

                    Effect.server.Run(
                        "assets/bundled/prefabs/fx/build/promote_stone.prefab",
                        entity.transform.position
                    );

                    if (this == null || !isBuildingState)
                    {
                        _ = ownIns.allObjectsOfWalls.Remove(entity.net.ID.Value);
                        _ = objectsOfWalls.Remove(entity);
                        entity.Kill();
                    }
                }

                private string GetRandomSignName()
                {
                    int i = UnityEngine.Random.Range(0, 9);
                    return strRoadSigns.ElementAt(i);
                }

                internal bool IsWagonOnMonument(TrainCar trainCar)
                {
                    Vector3 pos = branch.transform.position;
                    HashSet<TrainCar> trainCarsInside = GetEntities<TrainCar>(
                        new Vector3(pos.x, pos.y, pos.z + 0.6f),
                        14f
                    );

                    return trainCarsInside.Contains(trainCar);
                }

                private Wagon? GetWagon(TrainCar trainCar)
                {
                    return ownIns.allTrainCars.TryGetValue(trainCar.net.ID.Value, out Wagon wagon)
                        ? wagon
                        : null;
                }

                internal void Destroy()
                {
                    DestroyWalls();
                    button.Kill();
                }
            }

            internal sealed class PointAnimationTransform
            {
                public Vector3 Pos;
                public Vector3 Rot;
            }

            internal sealed class AnimationTransformVehicle : FacepunchBehaviour
            {
                internal BasePlayer? whoTryMoveWagon { get; set; }
                internal CustomMonument? customMonument { get; set; }
                private BaseEntity? Main { get; set; }

                private List<PointAnimationTransform> Path { get; } = new();

                private float SecondsTaken { get; set; }
                private float SecondsToTake { get; set; }
                private float WaypointDone { get; set; }

                private Vector3 StartPos { get; set; } = Vector3.zero;
                private Vector3 EndPos { get; set; } = Vector3.zero;

                private Vector3 StartRot { get; set; } = Vector3.zero;
                private Vector3 EndRot { get; set; } = Vector3.zero;

                private float Speed { get; set; }

                private void Awake()
                {
                    Main = GetComponent<BaseEntity>();
                    enabled = false;
                }

                internal void AddPath(PointAnimationTransform point, float speed)
                {
                    Path.Add(point);
                    Speed = speed;
                    enabled = true;
                }

                private void FixedUpdate()
                {
                    if (SecondsTaken == 0f)
                    {
                        if (Path.Count == 0)
                        {
                            StartPos = EndPos = Vector3.zero;
                            StartRot = EndRot = Vector3.zero;
                            SecondsToTake = 0f;
                            SecondsTaken = 0f;
                            WaypointDone = 0f;
                            enabled = false;

                            UpdatePosTrainCar();
                            customMonument.TryEnabledBuildingState(whoTryMoveWagon);

                            return;
                        }
                        StartPos = transform.position;
                        StartRot = transform.rotation.eulerAngles;
                        if (Path[0].Pos != StartPos || Path[0].Rot != StartRot)
                        {
                            EndPos = Path[0].Pos != StartPos ? Path[0].Pos : StartPos;
                            EndRot = Path[0].Rot != StartRot ? Path[0].Rot : StartRot;
                            SecondsToTake = Vector3.Distance(EndPos, StartPos) / Speed;
                            SecondsTaken = 0f;
                            WaypointDone = 0f;
                        }
                        Path.RemoveAt(0);
                    }
                    if (StartPos != EndPos || StartRot != EndRot)
                    {
                        SecondsTaken += UnityEngine.Time.deltaTime;
                        WaypointDone = Mathf.InverseLerp(0f, SecondsToTake, SecondsTaken);
                        if (StartPos != EndPos)
                        {
                            transform.position = Vector3.Lerp(StartPos, EndPos, WaypointDone);
                        }

                        if (StartRot != EndRot)
                        {
                            transform.rotation = Quaternion.Lerp(
                                Quaternion.Euler(StartRot),
                                Quaternion.Euler(EndRot),
                                WaypointDone
                            );
                        }

                        Main.TransformChanged();
                        Main.SendNetworkUpdate();
                        if (WaypointDone >= 1f)
                        {
                            SecondsTaken = 0f;
                        }
                    }
                }

                private void UpdatePosTrainCar()
                {
                    TrainCar trainCar = Main as TrainCar;
                    if (
                        !TrainTrackSpline.TryFindTrackNear(
                            trainCar!.GetFrontWheelPos(),
                            15f,
                            out TrainTrackSpline splineResult,
                            out float distResult
                        )
                    )
                    {
                        return;
                    }

                    trainCar.FrontWheelSplineDist = distResult;
                    Vector3 positionAndTangent = splineResult.GetPositionAndTangent(
                        trainCar.FrontWheelSplineDist,
                        transform.forward,
                        out Vector3 tangent
                    );
                    trainCar.SetTheRestFromFrontWheelData(
                        ref splineResult,
                        positionAndTangent,
                        tangent,
                        trainCar.localTrackSelection,
                        null,
                        instantMove: true
                    );
                    trainCar.FrontTrackSection = splineResult;
                }
            }

            #endregion Classes

            #region NTeleportation

            private void OnPlayerTeleported(BasePlayer player, Vector3 oldPos, Vector3 newPos)
            {
                if (playersOnMonuments.TryGetValue(player, out CustomMonument monument))
                {
                    monument.OnExitPlayer(player);
                }
            }

            #endregion NTeleportation

            #region MoveItem
            private static void MoveItem(BasePlayer player, Item item)
            {
                _ = ownIns.timer.In(
                    1f,
                    () =>
                    {
                        int spaceCountItem = GetSpaceCountItem(
                            player,
                            item.info.shortname,
                            item.MaxStackable(),
                            item.skin
                        );
                        int inventoryItemCount =
                            spaceCountItem > item.amount ? item.amount : spaceCountItem;

                        if (inventoryItemCount > 0)
                        {
                            Item itemInventory = ItemManager.CreateByName(
                                item.info.shortname,
                                inventoryItemCount,
                                item.skin
                            );
                            if (item.skin != 0)
                            {
                                itemInventory.name = item.name;
                            }

                            item.amount -= inventoryItemCount;
                            MoveInventoryItem(player, itemInventory);
                        }

                        if (item.amount > 0)
                        {
                            MoveOutItem(player, item);
                        }
                    }
                );
            }

            private static int GetSpaceCountItem(
                BasePlayer player,
                string shortname,
                int stack,
                ulong skinID
            )
            {
                int slots =
                    player.inventory.containerMain.capacity + player.inventory.containerBelt.capacity;
                int taken =
                    player.inventory.containerMain.itemList.Count
                    + player.inventory.containerBelt.itemList.Count;
                int result = (slots - taken) * stack;
                List<Item> allItems = Pool.Get<List<Item>>();
                _ = player.inventory.GetAllItems(allItems);
                foreach (Item item in allItems)
                {
                    if (item.info.shortname == shortname && item.skin == skinID && item.amount < stack)
                    {
                        result += stack - item.amount;
                    }
                }

                Pool.FreeUnmanaged(ref allItems);
                return result;
            }

            private static void MoveInventoryItem(BasePlayer player, Item item)
            {
                if (item.amount <= item.MaxStackable())
                {
                    List<Item> allItems = Pool.Get<List<Item>>();
                    _ = player.inventory.GetAllItems(allItems);
                    foreach (Item itemInv in allItems)
                    {
                        if (
                            itemInv.info.shortname == item.info.shortname
                            && itemInv.skin == item.skin
                            && itemInv.amount < itemInv.MaxStackable()
                        )
                        {
                            if (itemInv.amount + item.amount <= itemInv.MaxStackable())
                            {
                                itemInv.amount += item.amount;
                                itemInv.MarkDirty();
                                Pool.FreeUnmanaged(ref allItems);
                                return;
                            }
                            item.amount -= itemInv.MaxStackable() - itemInv.amount;

                            itemInv.amount = itemInv.MaxStackable();
                        }
                    }

                    Pool.FreeUnmanaged(ref allItems);
                    if (item.amount > 0)
                    {
                        _ = player.inventory.GiveItem(item);
                    }
                }
                else
                {
                    while (item.amount > item.MaxStackable())
                    {
                        Item thisItem = ItemManager.CreateByName(
                            item.info.shortname,
                            item.MaxStackable(),
                            item.skin
                        );
                        if (item.skin != 0)
                        {
                            thisItem.name = item.name;
                        }

                        _ = player.inventory.GiveItem(thisItem);
                        item.amount -= item.MaxStackable();
                    }
                    if (item.amount > 0)
                    {
                        _ = player.inventory.GiveItem(item);
                    }
                }
            }

            private static void MoveOutItem(BasePlayer player, Item item)
            {
                if (item.amount <= item.MaxStackable())
                {
                    _ = item.Drop(player.transform.position, Vector3.up);
                }
                else
                {
                    while (item.amount > item.MaxStackable())
                    {
                        Item thisItem = ItemManager.CreateByName(
                            item.info.shortname,
                            item.MaxStackable(),
                            item.skin
                        );
                        if (item.skin != 0)
                        {
                            thisItem.name = item.name;
                        }

                        _ = thisItem.Drop(player.transform.position, Vector3.up);
                        item.amount -= item.MaxStackable();
                    }
                    if (item.amount > 0)
                    {
                        _ = item.Drop(player.transform.position, Vector3.up);
                    }
                }
            }
            #endregion MoveItem

            #region Commands

            [ConsoleCommand("clearallwagons")]
            private void ClearAllWagons(ConsoleSystem.Arg arg)
            {
                if (arg.Player() != null)
                {
                    return;
                }

                int amount = wagons.Count;
                foreach (Wagon wagon in wagons.ToHashSet())
                {
                    wagon.RemoveWagon();
                }

                PrintWarning($"All custom wagons have been successfully removed! Count: {amount}");
            }

            [ConsoleCommand("givewagon")]
            private void GiveWagon(ConsoleSystem.Arg arg)
            {
                if (arg.Args == null || arg.Args.Length != 2 || arg.Player() != null)
                {
                    Puts("Incorrect syntax!");
                    Puts("Example: givewagon 10 76561145679474806");
                    return;
                }

                int amount = Convert.ToInt32(arg.Args[0]);
                string str = arg.Args[1];
                BasePlayer target = BasePlayer.Find(str);
                if (target == null)
                {
                    Puts($"Player {str} not found!");
                    return;
                }
                Puts("The wagon was successfully given to the player!");
                GiveWagon(target, amount);
            }

            [ChatCommand("showfreewagons")]
            private void ShowFreeWagons(BasePlayer player, string command, string[] args)
            {
                if (!permission.UserHasPermission(player.UserIDString, PermissionManager.freeWagons))
                {
                    NotifyManager.SendMessageToPlayer(
                        player,
                        "NotHavePerm",
                        ownIns._config.NotifyConfig.prefix
                    );
                    return;
                }

                ControllerSpawnsFreeWagon.ShowPosFreeWagons(player);
            }

            [ChatCommand("givewagon")]
            private void GiveWagonForAdmin(BasePlayer player, string command, string[] args)
            {
                if (!permission.UserHasPermission(player.UserIDString, PermissionManager.giveWagon))
                {
                    NotifyManager.SendMessageToPlayer(
                        player,
                        "NotHavePerm",
                        ownIns._config.NotifyConfig.prefix
                    );
                    return;
                }

                if (args == null || args.Length == 0)
                {
                    NotifyManager.SendMessageToPlayer(
                        player,
                        "InvalidCmdChat1",
                        ownIns._config.NotifyConfig.prefix
                    );
                    return;
                }

                if (args.Length == 1)
                {
                    NotifyManager.SendMessageToPlayer(
                        player,
                        "InvalidCmdChat2",
                        ownIns._config.NotifyConfig.prefix
                    );
                    return;
                }

                int amount = Convert.ToInt32(args[0]);
                string str = args[1];
                BasePlayer target = BasePlayer.Find(str);
                if (target == null)
                {
                    NotifyManager.SendMessageToPlayer(
                        player,
                        "InvalidCmdChat3",
                        ownIns._config.NotifyConfig.prefix,
                        str
                    );
                    return;
                }

                NotifyManager.SendMessageToPlayer(
                    player,
                    "ValidCmdChat",
                    ownIns._config.NotifyConfig.prefix
                );
                GiveWagon(player, amount);
            }

            [ChatCommand("removewagon")]
            private void RemoveWagon(BasePlayer player, string command, string[] args)
            {
                _ = playersWhoWantsToRemoveBaseWagon.Add(player);

                _ = timer.In(
                    5f,
                    () =>
                    {
                        if (playersWhoWantsToRemoveBaseWagon.Contains(player))
                        {
                            _ = playersWhoWantsToRemoveBaseWagon.Remove(player);
                        }

                        NotifyManager.SendMessageToPlayer(
                            player,
                            "RemoveTimeEnd",
                            ownIns._config.NotifyConfig.prefix
                        );
                    }
                );
            }

            [ChatCommand("thinstruction")]
            private void Instruction(BasePlayer player, string message, string[] arg)
            {
                GUIManager.CreateGUI(player, GUIManager.GUITopic.Base);
            }

            #endregion Commands

            #region API

            [HookMethod("IsEntityFromBaseWagon")]
            private bool IsEntityFromBaseWagon(ulong netIdValue)
            {
                return allBaseEntityByWagons.ContainsKey(netIdValue);
            }

            [HookMethod("IsBaseWagon")]
            private bool IsBaseWagon(ulong netIdValue)
            {
                return allParents.ContainsKey(netIdValue);
            }

            [HookMethod("IsTrainHomes")]
            private bool IsTrainHomes(ulong netIdValue)
            {
                return allTrainCars.ContainsKey(netIdValue);
            }

            [HookMethod("IsFreeWagon")]
            private bool IsFreeWagon(ulong netIdValue)
            {
                return ControllerSpawnsFreeWagon.freeWagons.Any(w => w.net.ID.Value == netIdValue);
            }

            #endregion API
        }
    }

    namespace Oxide.Plugins.TrainHomesExtensionMethods
    {
        public static class ExtensionMethods
        {
            public static HashSet<TSource> Where<TSource>(
                this IEnumerable<TSource> source,
                Func<TSource, bool> predicate
            )
            {
                HashSet<TSource> result = new();
                using IEnumerator<TSource> enumerator = source.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (predicate(enumerator.Current))
                    {
                        _ = result.Add(enumerator.Current);
                    }
                }

                return result;
            }

            public static int GetCountVariable<TSource>(
                this IEnumerable<TSource> source,
                Func<TSource, bool> predicate
            )
            {
                int result = 0;
                using IEnumerator<TSource> enumerator = source.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (predicate(enumerator.Current))
                    {
                        result++;
                    }
                }

                return result;
            }

            public static TSource? FirstOrDefault<TSource>(
                this IEnumerable<TSource> source,
                Func<TSource, bool> predicate
            )
            {
                using (IEnumerator<TSource> enumerator = source.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (predicate(enumerator.Current))
                        {
                            return enumerator.Current;
                        }
                    }
                }
                return default;
            }

            public static HashSet<TResult> Select<TSource, TResult>(
                this IEnumerable<TSource> source,
                Func<TSource, TResult> predicate
            )
            {
                HashSet<TResult> result = new();
                using IEnumerator<TSource> enumerator = source.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    _ = result.Add(predicate(enumerator.Current));
                }

                return result;
            }

            public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
            {
                List<TSource> result = new();
                using IEnumerator<TSource> enumerator = source.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    result.Add(enumerator.Current);
                }

                return result;
            }

            public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source)
            {
                HashSet<TSource> result = new();
                using IEnumerator<TSource> enumerator = source.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    _ = result.Add(enumerator.Current);
                }

                return result;
            }

            public static TSource? ElementAt<TSource>(this IEnumerable<TSource> source, int index)
            {
                int movements = 0;
                using (IEnumerator<TSource> enumerator = source.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (movements == index)
                        {
                            return enumerator.Current;
                        }

                        movements++;
                    }
                }
                return default;
            }

            public static bool IsPlayer(this BasePlayer player)
            {
                return player?.userID.IsSteamId() == true;
            }

            public static bool IsExists(this BaseNetworkable entity)
            {
                return entity?.IsDestroyed == false;
            }
        }
    }
}