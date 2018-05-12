namespace Pandaros.Settlers
{
    public enum DamageType
    {
        Physical,
        Earth,
        Air,
        Fire,
        Water,
        Void
    }

    public static class DamageTypeExtentions
    {
        public static float CalcDamage(this DamageType sourceType, DamageType monsterType, float damage)
        {
            float multiplier = 1;

            switch (sourceType)
            {
                case DamageType.Air:

                    switch (monsterType)
                    {
                        case DamageType.Air:
                            multiplier = 0;
                            break;

                        case DamageType.Fire:
                            multiplier = 0.5f;
                            break;

                        case DamageType.Water:
                            multiplier = 2;
                            break;

                        case DamageType.Earth:
                            multiplier = 0.5f;
                            break;
                    }

                    break;

                case DamageType.Fire:

                    switch (monsterType)
                    {
                        case DamageType.Air:
                            multiplier = 2;
                            break;

                        case DamageType.Fire:
                            multiplier = 0f;
                            break;

                        case DamageType.Water:
                            multiplier = 0.5f;
                            break;

                        case DamageType.Earth:
                            multiplier = 0.5f;
                            break;
                    }

                    break;

                case DamageType.Water:

                    switch (monsterType)
                    {
                        case DamageType.Air:
                            multiplier = 0.5f;
                            break;

                        case DamageType.Fire:
                            multiplier = 0.5f;
                            break;

                        case DamageType.Water:
                            multiplier = 0f;
                            break;

                        case DamageType.Earth:
                            multiplier = 2f;
                            break;
                    }

                    break;

                case DamageType.Earth:

                    switch (monsterType)
                    {
                        case DamageType.Air:
                            multiplier = 2f;
                            break;

                        case DamageType.Fire:
                            multiplier = 0.5f;
                            break;

                        case DamageType.Water:
                            multiplier = 0.5f;
                            break;

                        case DamageType.Earth:
                            multiplier = 0;
                            break;
                    }

                    break;

                case DamageType.Physical:

                    switch (monsterType)
                    {
                        case DamageType.Void:
                            multiplier = 0;
                            break;
                    }

                    break;
            }

            return damage * multiplier;
        }
    }
}