using System.Collections.Generic;

namespace TimWorld
{
    class Tim
    {
        public int x, y, z;
        public float hp, mp;
        public float reproductiveUrge;
        public float reproductiveCounter;

        public enum DIR
        {
            WEST,
            EAST,
            NORTH,
            SOUTH
        }
        public enum SUBDIR
        {
            STRAIGHT,
            UP,
            DOWN
        }
        public DIR dir;
        public SUBDIR subdir;
        public NN nn;
        public Items.Item left, right;

        public Tim(int X, int Y, int Z, DIR Dir, SUBDIR Subdir, NN Nn, float Hp, float Mp, Items.Item Left, Items.Item Right)
        {
            x = X; y = Y; z = Z;
            dir = Dir; subdir = Subdir;
            nn = Nn;
            hp = Hp; mp = Mp;
            reproductiveUrge = 64.0f; reproductiveCounter = 0.0f;
            left = Left; right = Right;
        }

        public void PickUp(Items.Item item)
        {
            if (left == new Items.Item())
            {
                left = item;
            }
            else if (right == new Items.Item())
            {
                right = item;
            }
            else
            {
                Drop(item, x, y, z);
            }
        }

        public void PickUp()
        {
            List<Items.Item> floorItems = new List<Items.Item>();
            foreach (Items.Item item in Game.items)
            {
                if (item.x == x && item.y == y && item.z == z)
                {
                    floorItems.Add(item);
                }
            }

            int one = Extra.random.Next(0, floorItems.Count);
            PickUp(floorItems[one]); floorItems.RemoveAt(one);

            int two = Extra.random.Next(0, floorItems.Count);
            PickUp(floorItems[two]); floorItems.RemoveAt(two);
        }

        public void Drop(Items.Item item, int X, int Y, int Z)
        {
            Game.items.Add(new Items.Item(item.item, item.count, X, Y, Z));
        }

        public void Use(bool ARM, int count)
        {
            if (ARM) // right
            {
                if (right.count > count) right = new Items.Item(right.item, right.count - count, x, y, z);
                else right = new Items.Item();
            }
            else // left
            {
                if (left.count > count) left = new Items.Item(left.item, left.count - count, x, y, z);
                else left = new Items.Item();
            }
        }

        // Tim's working neurons' order: ear, 64*(4)eyes, 6*(4)hairs, turn_left, turn_right, turn_up, turn_down, arm1/arm2, attack/interact, operand, mouth, move forward, move backward, move left, move right, jump.
        // TOADD entity system: pig and zombie
        // TOADD daynight cycle

        public void CalculateState()
        {
            int nPtr = 0;
            //*sensing: ear, 64*(4)eyes, 6*(4)hairs
            // TODO ear

            // vvv OPTIMIZE vvv still has TODO
            #region Hairs
            byte[] blockNeurons;
            blockNeurons = SpecialMath.NibbleToNeurons(Game.map[SpecialMath.Modulus(x + 1, Game.MapWidth), y, z]); // TODO add index of player with offset (TODO implement dir)
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++) nn.neurons[nPtr + j].value = blockNeurons[j];
                nPtr += 4;
            }

            blockNeurons = SpecialMath.NibbleToNeurons(Game.map[SpecialMath.Modulus(x - 1, Game.MapWidth), y, z]); // TODO add index of player with offset
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++) nn.neurons[nPtr + j].value = blockNeurons[j];
                nPtr += 4;
            }

            blockNeurons = SpecialMath.NibbleToNeurons(Game.map[x, SpecialMath.Modulus(y + 1, Game.MapHeight), z]); // TODO add index of player with offset
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++) nn.neurons[nPtr + j].value = blockNeurons[j];
                nPtr += 4;
            }

            blockNeurons = SpecialMath.NibbleToNeurons(Game.map[x, SpecialMath.Modulus(y - 1, Game.MapHeight), z]); // TODO add index of player with offset
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++) nn.neurons[nPtr + j].value = blockNeurons[j];
                nPtr += 4;
            }

            blockNeurons = SpecialMath.NibbleToNeurons(Game.map[x, y, SpecialMath.Modulus(z + 1, Game.MapLength)]); // TODO add index of player with offset
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++) nn.neurons[nPtr + j].value = blockNeurons[j];
                nPtr += 4;
            }

            blockNeurons = SpecialMath.NibbleToNeurons(Game.map[x, y, SpecialMath.Modulus(z - 1, Game.MapLength)]); // TODO add index of player with offset
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++) nn.neurons[nPtr + j].value = blockNeurons[j];
                nPtr += 4;
            }
            #endregion

            // vvv TODO
            #region Eyes
            #endregion

            //*nn iteration (lol ez)
            nn.Iterate();

            //*interaction: turn_LR, turn_UD, arm1/arm2, attack/interact, operand, mouth
            #region Directions
            float turn_LR = nn.neurons[nPtr].value; nPtr++;
            float turn_UD = nn.neurons[nPtr].value; nPtr++;

            dir    = Extra.DirectionCycle(dir, true && turn_LR >= 0);
            subdir = Extra.SubDirectionCycle(subdir, true && turn_UD >= 0);
            #endregion

            #region Interaction
            // evaluate coord
            int[] coord = Extra.DirectionToCoordinate(dir, subdir);
            int[] timcoord = new int[] { x, y, z };
            int[] limits = new int[] { Game.MapWidth, Game.MapHeight, Game.MapLength };

            int[] targetcoord = new int[] { 0, 0, 0 };
            for (int i = 0; i < 3; i++) targetcoord[i] = SpecialMath.Modulus(coord[i] + timcoord[i], limits[i]);

            // big switch part
            bool ARM = false; // if right arm or left arm
            float fARM = nn.neurons[nPtr].value; nPtr++;
            bool AI = false; // attack or interact
            float fAI = nn.neurons[nPtr].value; nPtr++;
            Items.Item armItem;

            // evaluate ARM, AI and armItem
            ARM = true && fARM >= 0;
            AI  = true && fAI  >= 0;
            armItem = ARM ? right : left;

            // switch statement
            switch (AI)
            {
                case true: // attack // still has TODO
                    switch (Game.map[targetcoord[0], targetcoord[1], targetcoord[2]])
                    {
                        case (byte)Blocks.Block.Air: // TODO attack player
                            break;

                        case (byte)Blocks.Block.Stone:
                            if (armItem.item == Items.ItemEnum.WoodPickaxe || armItem.item == Items.ItemEnum.MetalPickaxe)
                            {
                                Drop(new Items.Item(Items.ItemEnum.Stone, 1, targetcoord[0], targetcoord[1], targetcoord[2]), targetcoord[0], targetcoord[1], targetcoord[2]);
                                Game.map[targetcoord[0], targetcoord[1], targetcoord[2]] = (byte)Blocks.Block.Air;
                            }
                            break;

                        case (byte)Blocks.Block.Dirt:
                            Drop(new Items.Item(Items.ItemEnum.Dirt, 1, targetcoord[0], targetcoord[1], targetcoord[2]), targetcoord[0], targetcoord[1], targetcoord[2]);
                            Game.map[targetcoord[0], targetcoord[1], targetcoord[2]] = (byte)Blocks.Block.Air;
                            break;

                        case (byte)Blocks.Block.Tree:
                            Drop(new Items.Item(Items.ItemEnum.Tree, 1, targetcoord[0], targetcoord[1], targetcoord[2]), targetcoord[0], targetcoord[1], targetcoord[2]);
                            Game.map[targetcoord[0], targetcoord[1], targetcoord[2]] = (byte)Blocks.Block.Air;
                            break;

                        case (byte)Blocks.Block.Metal:
                            if (armItem.item == Items.ItemEnum.WoodPickaxe || armItem.item == Items.ItemEnum.MetalPickaxe)
                            {
                                Drop(new Items.Item(Items.ItemEnum.Metal, 1, targetcoord[0], targetcoord[1], targetcoord[2]), targetcoord[0], targetcoord[1], targetcoord[2]);
                                Game.map[targetcoord[0], targetcoord[1], targetcoord[2]] = (byte)Blocks.Block.Air;
                            }
                            break;

                        case (byte)Blocks.Block.Table:
                            Drop(new Items.Item(Items.ItemEnum.Table, 1, targetcoord[0], targetcoord[1], targetcoord[2]), targetcoord[0], targetcoord[1], targetcoord[2]);
                            Game.map[targetcoord[0], targetcoord[1], targetcoord[2]] = (byte)Blocks.Block.Air;
                            break;

                        case (byte)Blocks.Block.Stack:
                            Drop(new Items.Item(Items.ItemEnum.Stack, 1, targetcoord[0], targetcoord[1], targetcoord[2]), targetcoord[0], targetcoord[1], targetcoord[2]);

                            int index = Items.Stack.FindIndex(Game.stacks, targetcoord[0], targetcoord[1], targetcoord[2]);
                            if (Game.stacks[index].items.Count > 0)
                            {
                                for (int i = 0; i < Game.stacks[index].items.Count; i++)
                                {
                                    Drop(Game.stacks[index].items[i], targetcoord[0], targetcoord[1], targetcoord[2]);
                                }
                            }

                            Game.map[targetcoord[0], targetcoord[1], targetcoord[2]] = (byte)Blocks.Block.Air;
                            Game.stacks.RemoveAt(index);
                            break;

                        case (byte)Blocks.Block.Water: // useless lol
                            break;

                        case (byte)Blocks.Block.FarmLand:
                            Drop(new Items.Item(Items.ItemEnum.FarmLand, 1, targetcoord[0], targetcoord[1], targetcoord[2]), targetcoord[0], targetcoord[1], targetcoord[2]);

                            Game.map[targetcoord[0], targetcoord[1], targetcoord[2]] = (byte)Blocks.Block.Air;
                            Game.farmLands.RemoveAt(Items.FarmLand.FindIndex(Game.farmLands, targetcoord[0], targetcoord[1], targetcoord[2]));
                            break;

                        case (byte)Blocks.Block.Planks:
                            Drop(new Items.Item(Items.ItemEnum.Planks, 1, targetcoord[0], targetcoord[1], targetcoord[2]), targetcoord[0], targetcoord[1], targetcoord[2]);
                            Game.map[targetcoord[0], targetcoord[1], targetcoord[2]] = (byte)Blocks.Block.Air;
                            break;
                    }
                    break;

                case false: // interact
                    // evaluate operand
                    byte operand = 0;
                    operand = SpecialMath.FloatToByte(new float[4] { nn.neurons[nPtr].value, nn.neurons[nPtr + 1].value, nn.neurons[nPtr + 2].value, nn.neurons[nPtr + 3].value });
                    nPtr += 4;

                    switch (Game.map[targetcoord[0], targetcoord[1], targetcoord[2]])
                    {
                        case (byte)Blocks.Block.Air: // place block
                            // check if block is supported
                            bool supported = false;

                            byte[] blocks = new byte[6];
                            blocks[0] = Game.map[SpecialMath.Modulus(targetcoord[0] + 1, Game.MapWidth ), targetcoord[1], targetcoord[2]];
                            blocks[1] = Game.map[SpecialMath.Modulus(targetcoord[0] - 1, Game.MapWidth ), targetcoord[1], targetcoord[2]];
                            blocks[2] = Game.map[targetcoord[0], SpecialMath.Modulus(targetcoord[1] + 1, Game.MapHeight), targetcoord[2]];
                            blocks[3] = Game.map[targetcoord[0], SpecialMath.Modulus(targetcoord[1] - 1, Game.MapHeight), targetcoord[2]];
                            blocks[4] = Game.map[targetcoord[0], targetcoord[1], SpecialMath.Modulus(targetcoord[2] + 1, Game.MapLength)];
                            blocks[5] = Game.map[targetcoord[0], targetcoord[1], SpecialMath.Modulus(targetcoord[2] - 1, Game.MapLength)];

                            for (int i = 0; i < 6; i++)
                            {
                                if (blocks[i] != 0 && blocks[i] != 5)
                                {
                                    supported = true;
                                    break;
                                }
                            }

                            // place block
                            if (supported == true && (byte)armItem.item <= 9)
                            {
                                Game.map[targetcoord[0], targetcoord[1], targetcoord[2]] = (byte)armItem.item;

                                switch ((byte)armItem.item) // handle stack and farmLand
                                {
                                    case 6: // stack
                                        Game.stacks.Add(new Items.Stack(new List<Items.Item>(), targetcoord[0], targetcoord[1], targetcoord[2]));
                                        break;

                                    case 8: // farmland
                                        Game.farmLands.Add(new Items.FarmLand(targetcoord[0], targetcoord[1], targetcoord[2]));
                                        break;
                                }

                                Use(ARM, 1); // use 1 item
                            }
                            
                            break;

                        case (byte)Blocks.Block.Stone: // useless lol
                            break;

                        case (byte)Blocks.Block.Dirt: // try plant tree with FarmFood

                            if (armItem.item == Items.ItemEnum.FarmFood)
                            {
                                // plant tree
                                if (Game.map[targetcoord[0], SpecialMath.Modulus(targetcoord[1] + 1, Game.MapHeight), targetcoord[2]] == 0) 
                                    Game.map[targetcoord[0], SpecialMath.Modulus(targetcoord[1] + 1, Game.MapHeight), targetcoord[2]] = (byte)Blocks.Block.Tree; // trunk 1
                                if (Game.map[targetcoord[0], SpecialMath.Modulus(targetcoord[1] + 2, Game.MapHeight), targetcoord[2]] == 0)
                                    Game.map[targetcoord[0], SpecialMath.Modulus(targetcoord[1] + 2, Game.MapHeight), targetcoord[2]] = (byte)Blocks.Block.Tree; // trunk 2

                                // leaf algo (:D)
                                for (int i = 0; i < 3; i++)
                                {
                                    for (int j = 0; j < 2; j++)
                                    {
                                        for (int k = 0; k < 3; k++)
                                        {
                                            if (Game.map[SpecialMath.Modulus(targetcoord[0] + i, Game.MapWidth), SpecialMath.Modulus(targetcoord[1] + j + 1, Game.MapHeight), SpecialMath.Modulus(targetcoord[2] + k, Game.MapLength)] == 0)
                                                Game.map[SpecialMath.Modulus(targetcoord[0] + i, Game.MapWidth), SpecialMath.Modulus(targetcoord[1] + j + 1, Game.MapHeight), SpecialMath.Modulus(targetcoord[2] + k, Game.MapLength)] = (byte)Blocks.Block.Tree;
                                        }
                                    }
                                }

                                Use(ARM, 1); // use 1 FarmFood
                            }
                            break;

                        case (byte)Blocks.Block.Tree: // useless lol
                            break;

                        case (byte)Blocks.Block.Table: // crafting with operand
                            switch (operand)
                            {
                                case 9: // Planks
                                    if (armItem.item == Items.ItemEnum.Tree && armItem.count >= 1)
                                    {
                                        Use(ARM, 1); Drop(new Items.Item(Items.ItemEnum.Planks, 4, targetcoord[0], targetcoord[1], targetcoord[2]), targetcoord[0], targetcoord[1], targetcoord[2]);
                                    }
                                    break;

                                case 10: // WoodPickaxe
                                    if (armItem.item == Items.ItemEnum.Planks && armItem.count >= 2)
                                    {
                                        Use(ARM, 2); Drop(new Items.Item(Items.ItemEnum.WoodPickaxe, 1, targetcoord[0], targetcoord[1], targetcoord[2]), targetcoord[0], targetcoord[1], targetcoord[2]);
                                    }
                                    break;

                                case 12: // MetalPickaxe
                                    if (armItem.item == Items.ItemEnum.Metal && armItem.count >= 2)
                                    {
                                        Use(ARM, 2); Drop(new Items.Item(Items.ItemEnum.MetalPickaxe, 1, targetcoord[0], targetcoord[1], targetcoord[2]), targetcoord[0], targetcoord[1], targetcoord[2]);
                                    }
                                    break;

                                case 13: // WoodWeapon
                                    if (armItem.item == Items.ItemEnum.Planks && armItem.count >= 1)
                                    {
                                        Use(ARM, 1); Drop(new Items.Item(Items.ItemEnum.WoodWeapon, 1, targetcoord[0], targetcoord[1], targetcoord[2]), targetcoord[0], targetcoord[1], targetcoord[2]);
                                    }
                                    break;

                                case 14: // MetalWeapon
                                    if (armItem.item == Items.ItemEnum.Metal && armItem.count >= 1)
                                    {
                                        Use(ARM, 1); Drop(new Items.Item(Items.ItemEnum.MetalWeapon, 1, targetcoord[0], targetcoord[1], targetcoord[2]), targetcoord[0], targetcoord[1], targetcoord[2]);
                                    }
                                    break;
                            }
                            break;

                        case (byte)Blocks.Block.Stack: // grabbing/inserting
                            Items.Stack stack = Items.Stack.Find(Game.stacks, targetcoord[0], targetcoord[1], targetcoord[2]);

                            if (operand < 8)
                            { // grab
                                Drop(stack.Grab(), x, y, z); PickUp();
                            }
                            else
                            { // insert
                                stack.Insert(armItem); Use(ARM, armItem.count);
                            }
                            break;

                        case (byte)Blocks.Block.Water: // useless lol
                            break;

                        case (byte)Blocks.Block.FarmLand: // give FarmFood if grown
                            Items.FarmLand farmLand = Items.FarmLand.Find(Game.farmLands, targetcoord[0], targetcoord[1], targetcoord[2]);

                            if (farmLand.grown == true)
                            {
                                Drop(new Items.Item(Items.ItemEnum.FarmFood, 1, targetcoord[0], targetcoord[1], targetcoord[2]), targetcoord[0], targetcoord[1], targetcoord[2]);

                                Game.farmLands[Items.FarmLand.FindIndex(Game.farmLands, targetcoord[0], targetcoord[1], targetcoord[2])].Reset();
                            }
                            break;

                        case (byte)Blocks.Block.Planks: // useless lol
                            break;
                    }
                    break;
            }
            #endregion

            // TODO Mouth

            //*movement: move_left, move_right, move_forward, move_backward, jump

            #region Movement
            // evaluate moveLeft, moveRight, moveFoward, moveBackward and jump
            float moveLeft      = nn.neurons[nPtr].value; nPtr++;
            float moveRight     = nn.neurons[nPtr].value; nPtr++;
            float moveForward   = nn.neurons[nPtr].value; nPtr++;
            float moveBackward  = nn.neurons[nPtr].value; nPtr++;
            float jump         = nn.neurons[nPtr].value;
            bool  bJump          = true && jump >= 0;
            bool  moveSide      = true && moveLeft > moveRight;
            bool  moveStraight  = true && moveForward > moveBackward;
            bool  bMoveSide     = (true && moveLeft >= 0) ^ (true && moveRight >= 0);
            bool  bMoveStraight = (true && moveForward >= 0) ^ (true && moveBackward >= 0);

            // TODO jumping
            if (bJump) // jump
            {
                // level 1 jump

                // level 2 jump
            }

            // horizontal movement
            DIR movementDir = dir;

            if (bMoveStraight && !moveStraight) // alpha flip (0)
            {
                switch (movementDir)
                {
                    case DIR.NORTH:
                        movementDir = DIR.SOUTH;
                        break;

                    case DIR.SOUTH:
                        movementDir = DIR.NORTH;
                        break;

                    case DIR.EAST:
                        movementDir = DIR.WEST;
                        break;

                    case DIR.WEST:
                        movementDir = DIR.EAST;
                        break;
                }
            }

            int[] alpha = new int[] { 0, 0, 0 }; // no offset
            if (bMoveStraight) alpha = Extra.MovementDirectionToCoordinate(movementDir); // alpha coordinate (1)

            if (bMoveSide) movementDir = Extra.DirectionCycle(movementDir, moveSide); // beta cycle (2)

            int[] beta  = new int[] { 0, 0, 0 }; // no offset
            if (bMoveSide) beta = Extra.MovementDirectionToCoordinate(movementDir); // beta coordinate (3)

            int[] final = new int[] { 0, 0, 0 }; // initializing
            for (int i = 0; i < 3; i++) final[i] = alpha[i] + beta[i]; // final result (unofficial 4)

            if (Game.map[final[0], final[1], final[2]] != 0) // can move
            {
                x = final[0]; z = final[2];
            }
            #endregion
        }

        public void UpdateState()
        {
            // TODO

            // pick up items on the floor
            PickUp();

            // update items to have tim position
            left  = new Items.Item(left.item , left.count, x, y, z);
            right = new Items.Item(right.item, left.count, x, y, z);
        }
    }
}
