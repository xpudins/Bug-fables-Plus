using static MainManager;

namespace BFPlus.Extensions
{
    public class EnemyItemData
    {
        public static Items[][] enemyData =
        {
            // Zombiant
            new Items[] {(Items)NewItem.WhirlaRang, Items.HoneyDrop, Items.Mushroom, Items.AphidEgg },

            // Jellyshroom
            new Items[] {(Items)NewItem.WhirlaRang, Items.HoneyDrop, Items.Mushroom, Items.DangerShroom },

            // Spider
            null,

            // Zasp
            null,

            // Cactiling
            new Items[] {Items.CrunchyLeaf, Items.HoneyDrop, Items.BerryJuice, Items.ShockShroom, Items.JellyBean },

            // Psicorp
            new Items[] {Items.CrunchyLeaf, Items.RoastBerry, Items.BerryJuice, Items.ShockShroom },

            // Thief
            new Items[] { (Items)NewItem.WhirlaRang, (Items)NewItem.AgaricDots, (Items)NewItem.SpiderEyes, (Items)NewItem.TauntBerry, Items.HustleSeed, (Items)NewItem.SeedlingWhistle, (Items)NewItem.StickyBomb,(Items)NewItem.WebWad, (Items)NewItem.StickySoup, (Items)NewItem.Arachnomuffins, (Items)NewItem.Cottoncap, (Items)NewItem.BanditDelights },

            // Bandit
            new Items[] { (Items)NewItem.WhirlaRang, (Items)NewItem.AgaricDots, (Items)NewItem.SpiderEyes, Items.GlazedShroom, Items.HustleSeed, (Items)NewItem.SeedlingWhistle, (Items)NewItem.StickyBomb,(Items)NewItem.WebWad, (Items)NewItem.StickySoup, (Items)NewItem.Arachnomuffins, (Items)NewItem.Cottoncap, (Items)NewItem.BanditDelights  },

            // Inichas
            new Items[] {Items.CrunchyLeaf, Items.VitalitySeed, Items.Mushroom, Items.DangerShroom },

            // Seedling
            new Items[] {Items.CrunchyLeaf, Items.HoneyDrop, Items.VitalitySeed, Items.GenerousSeed, Items.HardSeed },

            // Flying Seedling
            new Items[] {Items.CrunchyLeaf, Items.HoneyDrop, Items.VitalitySeed, Items.GenerousSeed, Items.HardSeed },

            // Maki
            null,

            // Web
            null,

            // Spider
            null,

            // Numbnail
            new Items[] {Items.NumbDart, Items.GenerousSeed, Items.AphidEgg, (Items)NewItem.GoldenLeaf, (Items)NewItem.SlugPop, (Items)NewItem.SluggyDew },

            // Mothiva
            null,

            // Acornling
            new Items[] {Items.CrunchyLeaf, Items.HoneyDrop, Items.VitalitySeed, Items.GenerousSeed, Items.HardSeed },

            // Weevil
            new Items[] {Items.MagicDrops, Items.VitalitySeed, Items.GenerousSeed, (Items)NewItem.GoldenLeaf },

            // Mr. Tester
            null,

            // Venus' Bud
            new Items[] {Items.MagicDrops, Items.RoastBerry, Items.HardSeed, Items.ClearWater, (Items)NewItem.GoldenLeaf},

            // Chomper
            new Items[] {Items.CrunchyLeaf, Items.PoisonDart, Items.VitalitySeed, Items.GenerousSeed, Items.HardSeed, (Items)NewItem.GoldenLeaf },

            // Acolyte Aria
            null,

            // Vine
            null,

            // Kabbu
            null,

            // Venus' Guardian
            null,

            // Wasp Trooper
            new Items[] {Items.ProteinShake, Items.GlazedShroom, Items.VitalitySeed, Items.GenerousSeed, Items.NumbDart, Items.SpicyCandy, (Items)NewItem.SeedlingWhistle },

            // Wasp Bomber
            new Items[] { (Items)NewItem.WhirlyBomb, Items.BurlyBomb, Items.PoisonBomb, Items.FrostBomb, Items.NumbBomb, Items.SleepBomb, Items.ClearBomb },

            // Wasp Driller
            new Items[] {Items.SquashSoda, Items.SpicyBomb, Items.CookedLeaf, Items.BurlyTea, Items.SpicyTea, Items.MiteBurg, Items.BurlyChips, (Items)NewItem.SeedlingWhistle},

            // Wasp Scout
            new Items[] {Items.NumbBomb, Items.NumbDart, Items.PoisonDart, Items.PoisonBomb, Items.FrenchFries, (Items)NewItem.SeedlingWhistle },

            // Midge
            new Items[] {Items.CrunchyLeaf, Items.HoneyDrop, Items.VitalitySeed, Items.NumbDart, Items.JellyBean },

            // Underling
            new Items[] {Items.CrunchyLeaf, Items.HoneyDrop, Items.VitalitySeed, Items.GenerousSeed, Items.HardSeed, Items.PoisonSpud },

            // Monsieur Scarlet
            null,

            // Golden Seedling
            new Items[] {Items.TangyBerry, Items.TangyJam, Items.TangyPie, Items.TangyJuice, Items.TangyCarpaccio, (Items)NewItem.SeedlingWhistle },

            // Arrow Worm
            new Items[] {Items.PoisonSpud, Items.Squash, Items.ShockShroom, Items.DryBread},

            // Carmina
            null,

            // Seedling King
            null,

            // Broodmother
            null,

            // Plumpling
            new Items[] {Items.PlumplingPie, Items.SquashSoda, (Items)NewItem.SeedlingWhistle},

            // Flowerling
            new Items[] { (Items)NewItem.PointSwap, Items.RoastBerry, Items.MushroomCandy, Items.ClearBomb},

            // Burglar
            new Items[] { (Items)NewItem.WhirlaRang, (Items)NewItem.AgaricDots, (Items)NewItem.SpiderEyes, Items.GlazedShroom, Items.HustleSeed, (Items)NewItem.SeedlingWhistle, (Items)NewItem.StickyBomb,(Items)NewItem.WebWad, (Items)NewItem.StickySoup, (Items)NewItem.Arachnomuffins, (Items)NewItem.Cottoncap, (Items)NewItem.BanditDelights  },

            // Astotheles
            null,

            // Mother Chomper
            null,

            // Ahoneynation
            null,

            // Bee-Boop
            new Items[] { (Items)NewItem.BeeBattery, Items.GlazedHoney, Items.ShockShroom, Items.ShockCandy, Items.NumbDart, Items.RoastBerry },

            // Security Turret
            new Items[] {(Items)NewItem.BeeBattery,Items.GlazedHoney, Items.ShockShroom, Items.ShockCandy, Items.NumbBomb, Items.RoastBerry },

            // Denmuki
            new Items[] { (Items)NewItem.BeeBattery, Items.AphidEgg, Items.CrunchyLeaf, Items.ShockCandy, Items.ProteinShake, Items.RoastBerry },

            // Heavy Drone B-33
            null,

            // Mender
            null,

            // Abomihoney
            new Items[] {Items.GlazedHoney, Items.HoneyDrop, Items.Abomihoney, Items.Abombhoney},

            // Dune Scorpion
            null,

            // Tidal Wyrm
            null,

            // Kali
            null,

            // Zombee
            new Items[] { (Items)NewItem.PointSwap, Items.Mistake,Items.MushroomStick, Items.Mushroom, Items.MushroomCandy, Items.SpicyCandy },

            // Zombeetle
            new Items[] { (Items)NewItem.TauntBerry, (Items)NewItem.PointSwap, Items.Mistake,Items.MushroomStick, Items.Mushroom, Items.MushroomCandy, Items.BurlyCandy },

            // The Watcher
            null,

            // Peacock Spider
            null,

            // Bloatshroom
            new Items[] {Items.MushroomStick, Items.Mushroom, Items.MushroomCandy, Items.CookedShroom, Items.GlazedShroom, Items.FrostBomb },

            // Krawler
            new Items[] {Items.BerryJuice, Items.HustleSeed, Items.Ice },

            // Haunted Cloth
            new Items[] {Items.BerryJuice, Items.HustleSeed, Items.Ice },

            // Sand Wall
            null,

            // Ice Wall
            null,

            // Warden
            new Items[] {Items.BerryJuice, Items.HustleSeed, Items.Ice},

            // Wasp King
            null,

            // Jumping Spider
            new Items[] { (Items)NewItem.WhirlaRang, Items.LonglegSummoner, (Items)NewItem.Arachnomuffins, (Items)NewItem.StickyBomb, (Items)NewItem.WebWad, (Items)NewItem.Cottoncap},

            // Mimic Spider
            new Items[] {Items.LonglegSummoner, Items.Squash, Items.SquashSoda},

            // Leafbug Ninja
            new Items[] {(Items)NewItem.InkBomb, (Items)NewItem.LeafbugSkewer, (Items)NewItem.InkblotGravy,(Items)NewItem.MurkyPizza,(Items)NewItem.InkTrap,(Items)NewItem.SplotchScramble, (Items)NewItem.PaintedGourd, (Items)NewItem.InkySundae},

            // Leafbug Archer
            new Items[] {(Items)NewItem.InkBomb, (Items)NewItem.LeafbugSkewer, (Items)NewItem.InkblotGravy,(Items)NewItem.MurkyPizza,(Items)NewItem.InkTrap,(Items)NewItem.SplotchScramble, (Items)NewItem.PaintedGourd, (Items)NewItem.InkySundae },

            // Leafbug Clubber
            new Items[] {(Items)NewItem.InkBomb, (Items)NewItem.LeafbugSkewer, (Items)NewItem.InkblotGravy,(Items)NewItem.MurkyPizza,(Items)NewItem.InkTrap,(Items)NewItem.SplotchScramble, (Items)NewItem.PaintedGourd, (Items)NewItem.InkySundae },

            // Madesphy
            new Items[] { Items.HustleSeed, Items.HoneyDrop, Items.CrunchyLeaf, Items.VitalitySeed },

            // The Beast
            null,

            // Chomper Brute
            new Items[] {Items.NutCake, Items.DryBread, Items.BurlyChips, Items.SpicyCandy, Items.CoffeeCandy },

            // Mantidfly
            new Items[] {Items.CrunchyLeaf, Items.HustleSeed, Items.NumbDart, Items.VitalitySeed },

            // General Ultimax
            null,

            // Wild Chomper
            new Items[] {Items.CrunchyLeaf, Items.PoisonDart, Items.VitalitySeed, Items.GenerousSeed, Items.HardSeed },

            // Cross
            null,

            // Poi
            null,

            // Primal Weevil
            null,

            // False Monarch
            null,

            // Mothfly
            new Items[] { Items.CrunchyLeaf, Items.HoneyDrop, (Items)NewItem.InkBomb, (Items)NewItem.InkblotGravy,(Items)NewItem.MurkyPizza,(Items)NewItem.SplotchScramble, },

            // Mothfly Cluster
            new Items[] { Items.CookedLeaf, Items.GlazedHoney, (Items)NewItem.InkBomb, (Items)NewItem.InkblotGravy,(Items)NewItem.MurkyPizza,(Items)NewItem.SplotchScramble, },

            // Ironnail
            new Items[] { (Items)NewItem.SlugPop, (Items)NewItem.SluggyDew, Items.ProteinShake, Items.GenerousSeed, Items.AphidEgg },

            // Belostoss
            new Items[] {Items.ProteinShake, Items.GenerousSeed, Items.CookedLeaf },

            // Ruffian
            new Items[] {Items.SquashSoda, Items.DryBread, Items.CrunchyLeaf, (Items)NewItem.SeedlingWhistle },

            // Water Strider
            new Items[] {Items.LonglegSummoner},

            // Diving Spider
            new Items[] {Items.LonglegSummoner, Items.MagicDrops, (Items)NewItem.Arachnomuffins, (Items)NewItem.StickyBomb, (Items)NewItem.WebWad, (Items)NewItem.Cottoncap},

            // Cenn
            null,

            // Pisci
            null,

            // Dead Lander α
            null,

            // Dead Lander β
            null,

            // Dead Lander γ
            null,

            // Wasp King
            null,

            // The Everlasting King
            null,

            // Maki
            null,

            // Kina
            null,

            // Yin
            null,

            // ULTIMAX Tank
            null,

            // Zommoth
            null,

            // Riz
            null,

            // Devourer
            null,

            // Tail
            null,

            // Rock Wall
            null,

            // Ancient Key
            null,

            // Ancient Key
            null,

            // Ancient Tablet
            null,

            // Flytrap
            null,

            // FireKrawler
            new Items[] {Items.BerryJuice, Items.HustleSeed, Items.FlameRock,Items.Guarana },

            // FireWarden
            new Items[] {Items.BerryJuice, Items.HustleSeed, Items.FlameRock, Items.Guarana },

            // FireCape
            new Items[] {Items.BerryJuice, Items.HustleSeed,  Items.FlameRock,Items.Guarana },

            // IceKrawler
            new Items[] {Items.BerryJuice, Items.HustleSeed, Items.Ice },

            // IceWarden
            new Items[] {Items.BerryJuice, Items.HustleSeed, Items.Ice },

            // |glitchy,1|TANGYBUG|glitchy|
            null,

            // Stratos
            null,

            // Delilah
            null,
            // HoloVi
            null,

            // HoloKabbu
            null,

            // HoloLeif
            null,
            // Vi?
            null,

            // Kabbu?
            null,

            // Leif?
            null,

            // Caveling
            new Items[] {(Items)NewItem.WhirlaRang, Items.HoneyDrop, Items.VitalitySeed,(Items)NewItem.TauntBerry, Items.BerryJuice },

            // Flying Caveling
            new Items[] { (Items)NewItem.WhirlaRang, Items.HoneyDrop, Items.VitalitySeed, Items.GenerousSeed },

            // Frostfly
            new Items[] {Items.ShavedIce, Items.Ice, Items.HoneyIceCream, Items.SquashPie, Items.CookedJellyBean },

            // Pirahna Chomp
            new Items[] {Items.PoisonCake, Items.PoisonDart, Items.VitalitySeed, Items.GenerousSeed, Items.DangerShroom, (Items)NewItem.GoldenLeaf },

            // Moeruki
            new Items[] {Items.Omelet, Items.LeafSalad, Items.BurlyCandy, Items.ProteinShake, Items.RoastBerry, Items.FlameRock, (Items)NewItem.ViciousRose },

            // Mechajaw
            new Items[] { (Items)NewItem.TauntBerry, Items.BurlyTea, (Items)NewItem.AgaricDots, (Items)NewItem.JoltMush, Items.SquashSoda},

            // Splotch Spider
            new Items[] { Items.LonglegSummoner, (Items)NewItem.InkBomb, (Items)NewItem.InkblotGravy,(Items)NewItem.MurkyPizza,(Items)NewItem.InkTrap,(Items)NewItem.SplotchScramble, (Items)NewItem.InkySundae, (Items)NewItem.PaintedGourd, (Items)NewItem.InkyBrew, (Items)NewItem.BubbleHoney },

            // Worm
            new Items[] {Items.Squash, Items.AphidEgg, Items.AphidMilk, (Items)NewItem.TauntBerry},

            // Worm Swarm
            new Items[] {Items.Squash, Items.AphidEgg, Items.AphidMilk, (Items)NewItem.TauntBerry},

            // Spineling
            new Items[] {Items.CrunchyLeaf, Items.HoneyDrop, Items.BerryJuice, Items.PoisonDart, Items.JellyBean, (Items)NewItem.SeedlingWhistle },

            // Dewling
            new Items[] {Items.RoastBerry, Items.MushroomCandy, Items.ClearBomb, (Items)NewItem.SeedlingWhistle},

            // Fire Ant
            new Items[] {Items.Omelet, Items.CookedShroom, Items.CookedLeaf, Items.CookedJellyBean },

            // Belosslow
            null,

            // Dynamo Spore
            null,

            // Voltshroom
            new Items[] {Items.ShockShroom, Items.ShockCandy, Items.Mushroom },

            // Dull Scorp
            null,

            // Iron Suit
            null,

            // Mars
            null,

            // Mars Sprout
            null,

            // Levi
            null,

            // Celia
            null,

            //Mothmite
            new Items[] { (Items)NewItem.SlugPop, (Items)NewItem.SluggyDew },

            //Mars Bud
            new Items[] {Items.SpicyBomb, Items.BurlyBomb, Items.CherryPie, Items.Guarana, Items.BerryShake},
            
            //Termite Knight
            null,

            //Leafbug Shaman
            null,
            //Jester
            null,

            //Fire Popper
            new Items[] { Items.BurlyCandy, Items.ProteinShake, Items.RoastBerry, Items.FlameRock, (Items)NewItem.TauntBerry },
            
            //Patton
            null,

            //LongLegs
            new Items[] { Items.LonglegSummoner,(Items)NewItem.WebWad }
,
            //jump ant
            null,

            //red seedling
            null,

            //blue seedling
            null,
        };
    }

}
