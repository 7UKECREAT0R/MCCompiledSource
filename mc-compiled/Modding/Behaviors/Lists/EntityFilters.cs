using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors.Lists
{
    public class FilterClockTime : Filter
    {
        public const float NOON = 0.0f;
        public const float SUNSET = 0.25f;
        public const float MIDNIGHT = 0.5f;
        public const float SUNRISE = 0.75f;

        public float time; // 0.0 - 1.0
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "clock_time";
        public override object GetValue() => this.time;
    }
    public class FilterDistanceToNearestPlayer : Filter
    {
        public float distance;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "distance_to_nearest_player";
        public override object GetValue() => this.distance;
    }
    public class FilterHasAbility : Filter
    {
        public enum Ability
        {
            flyspeed, flying, instabuild, invulnerable, lightning, mayfly, mute, noclip, walkspeed, worldbuilder
        }

        public Ability ability; // 0.0 - 1.0
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "has_ability";
        public override object GetValue() => this.ability.ToString();
    }
    public class FilterHasBiomeTag : Filter
    {
        public string biomeTag;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "has_biome_tag";
        public override object GetValue() => this.biomeTag;
    }
    public class FilterHasComponent : Filter
    {
        public string componentName;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "has_component";
        public override object GetValue() => this.componentName;
    }
    public class FilterHasContainerOpen : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "has_container_open";
    }
    public class FilterHasDamage : Filter
    {
        public Commands.DamageCause damageType;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "has_damage";
        public override object GetValue() => this.damageType.ToString();
    }
    public class FilterHasEquipment : Filter
    {
        public enum Domain
        {
            any, armor, feet, hand, head, leg, torso
        }

        public string itemName;
        public Domain? itemDomain;
        public override JProperty[] GetExtraProperties()
        {
            if (this.itemDomain.HasValue)
                return new JProperty[]
                {
                    new JProperty("domain", this.itemDomain.ToString())
                };

            return null;
        }
        public override string GetTest() => "has_equipment";
        public override object GetValue() => this.itemName;
    }
    public class FilterHasMobEffect : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "has_mob_effect";
    }
    public class FilterHasNametag : Filter
    {
        public string nameTag;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "has_nametag";
        public override object GetValue() => this.nameTag;
    }
    public class FilterHasRangedWeapon : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "has_ranged_weapon";
    }
    public class FilterHasTag : Filter
    {
        public string tag;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "has_tag";
        public override object GetValue() => this.tag;
    }
    public class FilterHasTarget : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "has_target";
    }
    public class FilterHasTradeSupply : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "has_trade_supply";
    }
    public class FilterHourlyClockTime : Filter
    {
        public const int MIN_TIME = 0;
        public const int MAX_TIME = 24000;

        protected int time;
        public int Time
        {
            get => this.time;
            set
            {
                if (value > MAX_TIME)
                    value = MAX_TIME;
                if (value < MIN_TIME)
                    value = MIN_TIME;
                this.time = value;
            }
        }

        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "hourly_clock_time";
        public override object GetValue() => this.time;
    }
    public class FilterInCaravan : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "in_caravan";
    }
    public class FilterInClouds : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "in_clouds";
    }
    public class FilterInLava : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "in_lava";
    }
    public class FilterInNether : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "in_nether";
    }
    public class FilterInWater : BooleanFilter
    {
        public bool includeRain;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => this.includeRain ? "in_water_or_rain" : "in_water";
    }
    public class FilterInactivityTimer : Filter
    {
        public int inactiveFor;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "inactivity_timer";
        public override object GetValue() => this.inactiveFor;
    }
    public class FilterIsAltitude : Filter
    {
        public int yValue;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_altitude";
        public override object GetValue() => this.yValue;
    }
    public class FilterIsAvoidingMobs : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_avoiding_mobs";
    }
    public class FilterIsBiome : Filter
    {
        public string biome;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_biome";
        public override object GetValue() => this.biome;
    }
    public class FilterIsBlock : Filter
    {
        public string blockName;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_block";
        public override object GetValue() => this.blockName;
    }
    public class FilterIsBrightness : Filter
    {
        public float brightness; // 0.0 - 1.0
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_brightness";
        public override object GetValue() => this.brightness;
    }
    public class FilterIsClimbing : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_climbing";
    }
    public class FilterIsColor : Filter
    {
        public enum FilterColor
        {
            black,
            blue,
            brown,
            cyan,
            gray,
            green,
            light_blue,
            light_green,
            magenta,
            orange,
            pink,
            purple,
            red,
            silver,
            white,
            yellow
        }

        public FilterColor color;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_color";
        public override object GetValue() => this.color.ToString();
    }
    public class FilterIsDaytime : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_daytime";
    }
    public class FilterIsDifficulty : Filter
    {
        public Commands.DifficultyMode difficulty;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_difficulty";
        public override object GetValue() => this.difficulty.ToString();
    }
    public class FilterIsFamily : Filter
    {
        public string family;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_family";
        public override object GetValue() => this.family;
    }
    public class FilterIsGameRule : Filter
    {
        public Commands.GameRule gameRule;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_game_rule";
        public override object GetValue() => this.gameRule.ToString();
    }
    public class FilterIsHumid : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_humid";
    }
    public class FilterIsImmobile : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_immobile";
    }
    public class FilterIsInVillage : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_village";
    }
    public class FilterIsLeashed : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_leashed";
    }
    public class FilterIsLeashedTo : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_leashed_to";
    }
    public class FilterIsMarkVariant : Filter
    {
        public int markVariant;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_mark_variant";
        public override object GetValue() => this.markVariant;
    }
    public class FilterIsMoving : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_moving";
    }
    public class FilterIsOwner : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_owner";
    }
    public class FilterIsPersistent : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_persistent";
    }
    public class FilterIsRiding : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_riding";
    }
    public class FilterIsSkinID : Filter
    {
        public int skinID;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_skin_id";
        public override object GetValue() => this.skinID;
    }
    public class FilterIsSleeping : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_sleeping";
    }
    public class FilterIsSneaking : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_sneaking";
    }
    public class FilterIsSnowCovered : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_snow_covered";
    }
    public class FilterIsTarget : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_target";
    }
    public class FilterIsTemperatureType : Filter
    {
        public enum TemperatureType
        {
            cold, mild, ocean, warm
        }

        public TemperatureType type;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_temperature_type";
        public override object GetValue() => this.type.ToString();
    }
    public class FilterIsTemperatureValue : Filter
    {
        public float temperature; // 0.0 - 1.0
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_temperature_value";
        public override object GetValue() => this.temperature;
    }
    public class FilterIsUnderground : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_underground";
    }
    public class FilterIsUnderwater : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_underwater";
    }
    public class FilterIsVariant : Filter
    {
        public int variant;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_variant";
        public override object GetValue() => this.variant;
    }
    public class FilterIsVisible : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "is_visible";
    }
    public class FilterLightLevel : Filter
    {
        public int lightLevel;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "light_level";
        public override object GetValue() => this.lightLevel;
    }
    public class FilterMoonIntensity : Filter
    {
        public float moonIntensity; // 0.0 - 1.0
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "moon_intensity";
        public override object GetValue() => this.moonIntensity;
    }
    public class FilterMoonPhase : Filter
    {
        public const int MIN_PHASE = 0;
        public const int MAX_PHASE = 7;

        private int moonPhase;
        public int MoonPhase
        {
            get => this.moonPhase;
            set
            {
                if (value > MAX_PHASE)
                    value = MAX_PHASE;
                if (value < MIN_PHASE)
                    value = MIN_PHASE;
                this.moonPhase = value;
            }
        }

        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "moon_phase";
        public override object GetValue() => this.moonPhase;
    }
    public class FilterOnGround : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "on_ground";
    }
    public class FilterOnLadder : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "on_ladder";
    }
    public class FilterRandomChance : Filter
    {
        public int randomChance; // random(0..randomChance) == 0
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "random_chance";
        public override object GetValue() => this.randomChance;
    }
    public class FilterRiderCount : Filter
    {
        public int count;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "rider_count";
        public override object GetValue() => this.count;
    }
    public class FilterIsSurfaceMob : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "surface_mob";
    }
    public class FilterTrusts : BooleanFilter
    {
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => "trusts";
    }
    public class FilterWeather : Filter
    {
        public Commands.WeatherState weather;
        public bool atPosition;
        public override JProperty[] GetExtraProperties() => null;
        public override string GetTest() => this.atPosition ? "weather_at_position" : "weather";
        public override object GetValue() => this.weather.ToString();
    }
}