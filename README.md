# Settlers!
A colony survival settlers mod that helps you grow and manage your colony. Introduces new weapons, armor, research, difficulties, jobs and chat commands.

Food use now increases as colony size goes up to simulate waste.
Food use fluctuates depending on how hungry your colonists are!

**NOTE: With the release of 0.4.0 Only servers needs mods**

## Research!
![GitHub Logo](https://imgur.com/5ovugpE.png)
* Decreased food waste
* Increased Settler Health (All colonists, up to 50 at level 5)
* Additional Banners (Up to 20!)
* Armorsmithing
* Swordsmithing
* Knight Patrols
* Machinery
  * Improved Durability
  * Improved Fuel Capacity
  * Increased Item Capacity
* Improved Weapons (up to 25% increased reload speed at level 5)
  * Slings
  * Bows
  * Crossbows
  * Matchlockgun
 
## Game Difficulties!

The game defaults to meduim. Difficulty effects food consumption/waste and requirements for settlers, forcing you to meet the goals of getting settlers. (see Tips)

* /difficulty normal (Normal food use)
* /difficulty easy
* /difficulty medium
* /difficulty hard
* /difficulty insane

To see your current difficulty level type /difficulty

## Knight Patrols!
Once researched you can craft Patrol Banners at the workbench. Knight patrols require Swords. They will always use the best sword in the stockpile. Knights also get first pick (after the player) of armor. Knights are also tougher than other colonists and take 50% less damage before any armor is applied. Use the Patrol Tool (it goes into your stockpile after its researched) on your toolbar to do the following:

* Left Click: Place a Patrol Flag
* Right Click: Comfirm Patrol and start Job.

To remove a patrol simply destroy any banner in the patrol. It will return all banners from the patrol to your inventory.

##### NOTE Knight patrols will only check for monsters to fight at the Patrol Flags! Not while walking inbetween!

|Icon|Description|
|---|---|
|![tool](https://imgur.com/oglGxYw.png)|Knight Patrol Tool|
|![Flag](https://imgur.com/uprsiQI.png)|Knight Patrol Flag.|

#### Patrol Types:
You may left click on a existing banner control to change the patrol type:

|Type|Description|
|---|---|
|Round Robin|The patrol will go from the first flag to the last flag. Once the patrol reaches the last flag it will walk back to the first flag and start again. This is good for making circles.|
|Zipper|The Patrol will go from the first flag to the last flag. Once the patrol reaches the last flag it will go backwords back to the first flag. This is good for patrolling a line.|

## Armor!
Once crafted YOU and your colonists will automatically equipt the best armor. You will always equipt the best armor before your colonists. Settlers will keep 5 sets of armor in the Stockpile for knights and will give away any extra to the colony to wear.

To see equipt armor use the /armor command!

#### Damage Reduction:
||Copper|Bronze|Iron|Steel|
|---|---|---|---|---|
|Helm|5%|7%|9%|11%|
|Chest|10%|15%|20%|30%|
|Gauntlets|2.5%|4%|5.5%|7%|
|Leggings|7%|9%|11%|13%|
|Greaves|2.5%|4%|5.5%|7%|
|Shield|5%|7%|10%|12%|

Armor has durability! When hit a colonist will take a point of durability damage.
#### Durability Hit Chance:
||W\O Shield|W\Shield|
|---|---|---|
|Helm|10%|10%|
|Chest|45%|20%|
|Gauntlets|10%|5%|
|Leggings|25%|10%|
|Greaves|10%|5%|
|Shield||50%|

#### Durability:
||Copper|Bronze|Iron|Steel|
|---|---|---|---|---|
|Helm|15|20|30|40|
|Chest|25|30|40|50|
|Gauntlets|10|15|25|35|
|Leggings|20|25|35|40|
|Greaves|10|15|25|35|
|Shield|30|40|50|60|

## Weapons!
Once crafted YOU and your colonists can wield weapons! Currently these weapons are only used by colonists during call to arms. 

#### Damage:
||Copper|Bronze|Iron|Steel|
|---|---|---|---|---|
|Sword|50|100|250|500|

Weapons have durability. Each attack takes 1 point of duribility damage.
#### Durability:
||Copper|Bronze|Iron|Steel|
|---|---|---|---|---|
|Sword|50|75|100|150|

## Food!
More end-game foods to help feed your hungry colonists!

#### Recipies:
|Name|Food Value|Recipe|Produce Count|
|---|---|---|---|
|Berry Pancakes|4|Flour: 3, Berry: 2, Firewood|2|
|Berry Pie|6|Flour: 4, Berry: 4, Firewood|2|

## Machines!
![Machines](https://imgur.com/Q78m0B9.png)
Machines help your colonists out a bit! They need to be researched to be activated. 

#### Machine Types:
|Type|Description|Fuel|
|---|---|---|
|![Miner](https://imgur.com/sdIwB2Q.png)|The Mining Machine. Mines at 2x the rate of a settler|1 Firewood for 10% fuel or 1 coal for 20% fuel| 
|![StoneTurret](https://imgur.com/DgWcNtJ.png)|The Stone Turret. 50 Damage per shot. Fires twice as fast as a Colonist.|1 Firewood for 10% fuel or 1 coal for 20% fuel| 
|![BronzeArrowTurret](https://imgur.com/e65jmAR.png)|The Bronze Arrow Turret. 100 Damage per shot. Fires twice as fast as a Colonist.|1 Firewood for 10% fuel or 1 coal for 20% fuel| 
|![CrossbowTurret](https://imgur.com/3rvqDvA.png)|The Crossbow Turret. 300 Damage per shot. Fires twice as fast as a Colonist.|1 Firewood for 10% fuel or 1 coal for 20% fuel| 
|![MatchlockTurret](https://imgur.com/zTrZbJf.png)|The Matchlock Turret. 500 Damage per shot. Fires twice as fast as a Colonist.|1 Firewood for 10% fuel or 1 coal for 20% fuel| 

#### Machine Repairs:
The less damaged a machine the cheaper it is to fix:
<table>
    <tbody>
        <tr>
            <th>Icon</th>
            <th>75% or less</th>
            <th>50% or less</th>
            <th>30% or less</th>
            <th>10% or less</th>
        </tr>
        <tr>
            <td><img src="https://imgur.com/sdIwB2Q.png" /></td>
            <td>
                <ul>
                    <li>1x Planks</li>
                    <li>1x Iron Rivet</li>
                    <li>2x Copper Nails</li>
                    <li>1x Copper Tools</li>
                </ul>
            </td>
            <td>
                <ul>
                    <li>1x Planks</li>
                    <li>1x Iron Rivet</li>
                    <li>2x Copper Nails</li>
                    <li>1x Copper Tools</li>
                    <li>1x Copper Parts</li>
                </ul>
            </td>
            <td>
                <ul>
                    <li>1x Planks</li>
                    <li>1x Iron Rivet</li>
                    <li>2x Copper Nails</li>
                    <li>1x Copper Tools</li>
                    <li>1x Wrought Iron</li>
                    <li>2x Copper Parts</li>
                </ul>
            </td>
            <td>
                <ul>
                    <li>1x Planks</li>
                    <li>1x Iron Rivet</li>
                    <li>2x Copper Nails</li>
                    <li>1x Copper Tools</li>
                    <li>1x Wrought Iron</li>
                    <li>4x Copper Parts</li>
                </ul>
            </td>
        </tr>
        <tr>
            <td><img src="https://imgur.com/DgWcNtJ.png" /></td>
            <td>
                <ul>
                    <li>1x Stone Bricks</li>
                    <li>1x Planks</li>
                    <li>1x Copper Parts</li>
                    <li>2x Copper Nails</li>
                </ul>
            </td>
            <td>
                <ul>
                    <li>1x Stone Bricks</li>
                    <li>1x Planks</li>
                    <li>2x Copper Parts</li>
                    <li>4x Copper Nails</li>
                </ul>
            </td>
            <td>
                <ul>
                    <li>1x Stone Bricks</li>
                    <li>1x Planks</li>
                    <li>4x Copper Parts</li>
                    <li>4x Copper Nails</li>
                    <li>1x Sling</li>
                </ul>
            </td>
            <td>
                <ul>
                    <li>1x Stone Bricks</li>
                    <li>1x Planks</li>
                    <li>6x Copper Parts</li>
                    <li>6x Copper Nails</li>
                    <li>2x Sling</li>
                </ul>
            </td>
        </tr>
        <tr>
            <td><img src="https://imgur.com/e65jmAR.png" /></td>
            <td>
                <ul>
                    <li>1x Stone Bricks</li>
                    <li>1x Planks</li>
                    <li>1x Copper Parts</li>
                    <li>2x Copper Nails</li>
                </ul>
            </td>
            <td>
                <ul>
                    <li>1x Stone Bricks</li>
                    <li>1x Planks</li>
                    <li>2x Copper Parts</li>
                    <li>4x Copper Nails</li>
                </ul>
            </td>
            <td>
                <ul>
                    <li>1x Stone Bricks</li>
                    <li>1x Planks</li>
                    <li>4x Copper Parts</li>
                    <li>4x Copper Nails</li>
                    <li>1x Bow</li>
                </ul>
            </td>
            <td>
                <ul>
                    <li>1x Stone Bricks</li>
                    <li>1x Planks</li>
                    <li>6x Copper Parts</li>
                    <li>6x Copper Nails</li>
                    <li>2x Bow</li>
                </ul>
            </td>
        </tr>
        <tr>
            <td><img src="https://imgur.com/3rvqDvA.png" /></td>
            <td>
                <ul>
                    <li>1x Stone Bricks</li>
                    <li>1x Planks</li>
                    <li>1x Iron Rivet</li>
                    <li>2x Copper Nails</li>
                </ul>
            </td>
            <td>
                <ul>
                    <li>1x Stone Bricks</li>
                    <li>1x Planks</li>
                    <li>2x Iron Rivet</li>
                    <li>4x Copper Nails</li>
                </ul>
            </td>
            <td>
                <ul>
                    <li>1x Stone Bricks</li>
                    <li>1x Planks</li>
                    <li>4x Iron Rivet</li>
                    <li>4x Copper Nails</li>
                    <li>1x Crossbow</li>
                </ul>
            </td>
            <td>
                <ul>
                    <li>1x Stone Bricks</li>
                    <li>1x Planks</li>
                    <li>6x Iron Rivet</li>
                    <li>6x Copper Nails</li>
                    <li>2x Crossbow</li>
                </ul>
            </td>
        </tr>
        <tr>
            <td><img src="https://imgur.com/zTrZbJf.png" /></td>
            <td>
                <ul>
                    <li>1x Stone Bricks</li>
                    <li>1x Planks</li>
                    <li>1x Steel Parts</li>
                    <li>2x Copper Nails</li>
                </ul>
            </td>
            <td>
                <ul>
                    <li>1x Stone Bricks</li>
                    <li>1x Planks</li>
                    <li>2x Steel Parts</li>
                    <li>4x Copper Nails</li>
                </ul>
            </td>
            <td>
                <ul>
                    <li>1x Stone Bricks</li>
                    <li>1x Planks</li>
                    <li>4x Steel Parts</li>
                    <li>4x Copper Nails</li>
                    <li>1x Matchlock Gun</li>
                </ul>
            </td>
            <td>
                <ul>
                    <li>1x Stone Bricks</li>
                    <li>1x Planks</li>
                    <li>6x Steel Parts</li>
                    <li>6x Copper Nails</li>
                    <li>2x Matchlock Gun</li>
                </ul>
            </td>
        </tr>
    </tbody>
</table>

## Machinists!
Machines need Machinists nearby to repair, fuel an reload your machines. A Machinists bench must be within 20 blocks of a machine in order for the machinest to operate it. Machinists will repair a machine at 50% durability, refuel a machine when it drops below 30% of fuel or reload a machine when it drops below 30% of items it requires (such as ammo).

#### Indicators:
|Icon|Description|
|---|---|
|![Waiting](https://i.imgur.com/HF6tajn.png)|Waiting for a machine to need servicing.|
|![Repairing](https://imgur.com/ccxc98f.png)|Repairing the machine.|
|![Repairing](https://imgur.com/JGe8wNE.png)|Refueling the machine.|
|![Reloading](https://imgur.com/NncpHck.png)|Reloading the machine with items|

## Call to Arms!
![GitHub Logo](https://i.imgur.com/713Gqcp.png)
In trouble? Use one of the following commands:

* /call
* /arms
* /cta 

And ALL of your colonists will take up arms defend the colony! You must have weapons and ammo available for everyone! When activated, all colonists will stop working/wake up. They will go to the nearest stockpile and grab a weapon then start hunting monsters (100 block radius).

To turn it off simply type the command again and all colonists will go back to thier original jobs.

## To install!
Head on over to [Releases](https://github.com/JBurlison/Pandaros.Settlers/releases) and grab the latest version.

unzip into mods folder in Steam\steamapps\common\Colony Survival\gamedata\mods\

The mod should be in \Steam\steamapps\common\Colony Survival\gamedata\mods\Pandaros\Settlers\
