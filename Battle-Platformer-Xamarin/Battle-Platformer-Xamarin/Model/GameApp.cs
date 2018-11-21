﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Urho;
using Urho.Urho2D;
using System.Timers;
using System.Globalization;
using Battle_Platformer_Xamarin.Model;
using Urho.Audio;
using Urho.Gui;
using Battle_Platformer_Xamarin;

namespace Royale_Platformer.Model
{
    public class GameApp : Application
    {
        public static GameApp Instance { get; private set; }

        public CharacterPlayer PlayerCharacter { get; private set; }
        public List<Character> Characters { get; private set; }

        public List<Pickup> Pickups { get; set; }
        public List<Bullet> Bullets { get; set; }

        public List<MapTile> Tiles { get; set; }

        private List<WorldObject> collisionObjects;

        public bool LoadGame { get; set; }
        public Func<object> Restart { get; internal set; }

        private static readonly float bulletSpeed = 10f;

        private Scene scene;
        private Node cameraNode;
        private Sprite2D bulletSprite;
        private UIElement hud;
        private int time;
        private bool hardcore;
        private bool continueGame;
        Timer timer;
        
        public GameApp(ApplicationOptions options) : base(options)
        {
            Instance = this;
            string[] flags = options.AdditionalFlags.ToString().Split(',');

            hardcore = flags[0] == "True" ? true : false;
            continueGame = flags[1] == "True" ? true : false;

            Characters = new List<Character>();
            Pickups = new List<Pickup>();
            Bullets = new List<Bullet>();
            Tiles = new List<MapTile>();
            collisionObjects = new List<WorldObject>();
            LoadGame = false;
        }

        protected override void Start()
        {
            base.Start();

            float halfWidth = Graphics.Width * 0.5f * PixelSize;
            float halfHeight = Graphics.Height * 0.5f * PixelSize;

            // Create Scene
            scene = new Scene();
            scene.CreateComponent<Octree>();
            //scene.CreateComponent<PhysicsWorld2D>();

            cameraNode = scene.CreateChild("Camera");
            cameraNode.Position = new Vector3(0, 0, -10);

            Camera camera = cameraNode.CreateComponent<Camera>();
            camera.Orthographic = true;
            camera.OrthoSize = 2 * halfHeight;
            camera.Zoom = Math.Min(Graphics.Width / 1920.0f, Graphics.Height / 1080.0f);

            time = 6000;

            CreatePlayer(0, 0);
            if (!continueGame) CreateEnemies();
            CreateMap();
            PlayMusic();
            CreateHUD();
            CreateClock();

            bulletSprite = ResourceCache.GetSprite2D("map/levels/platformer-art-complete-pack-0/Request pack/Tiles/laserPurpleDot.png");
            if (bulletSprite == null)
                throw new Exception("Bullet sprite not found!");

            /*
            Bullets.Add(new Bullet(1, scene, bulletSprite, new Vector2(4, -2)));
            */

            // Setup Viewport
            Renderer.SetViewport(0, new Viewport(Context, scene, camera, null));
        }

        private void PlayMusic()
        {
            var music = ResourceCache.GetSound("sounds/loop1.ogg");
            music.Looped = true;
            Node musicNode = new Scene().CreateChild("Music");
            SoundSource musicSource = musicNode.CreateComponent<SoundSource>();
            musicSource.SetSoundType(SoundType.Music.ToString());
            musicSource.Play(music);
        }

        private void CreatePlayer(float x, float y)
        {
            //AnimatedSprite2D playerAnimatedSprite = playerNode.CreateComponent<AnimatedSprite2D>();
            //playerAnimatedSprite.BlendMode = BlendMode.Alpha;
            //playerAnimatedSprite.Sprite = playerSprite;

            var playerSprite = ResourceCache.GetSprite2D("characters/special forces/png1/attack/1_Special_forces_attack_Attack_000.png");
            if (playerSprite == null)
                throw new Exception("Player sprite not found");

            CharacterPlayer player = new CharacterPlayer(CharacterClass.Gunner, 10);
            player.CreateNode(scene, playerSprite, new Vector2(x, y));

            /*
            Input.MouseButtonDown += (args) =>
            {
                if(args.Button == 1)
                {
                    PlayerCharacter.Input.LeftClick = true;
                }
            };
            */

            AddPlayer(player);
        }

        private void CreateEnemies()
        {
            // "C:\Users\Elias\Documents\BJU\CPS209\Royale-Platformer\Battle-Platformer-Xamarin\Battle-Platformer-Xamarin.UWP\GameData\characters\special forces\png2\attack\2_Special_forces_attack_Attack_000.png"
            var enemySprite = ResourceCache.GetSprite2D("characters/special forces/png2/attack/2_Special_forces_attack_Attack_000.png");
            if (enemySprite == null)
                throw new Exception("Enemy sprite not found");

            CharacterEnemy enemy = new CharacterEnemy(CharacterClass.Support, 5);
            enemy.CreateNode(scene, enemySprite, new Vector2(4, -2));
            AddCharacter(enemy);

            CharacterEnemy enemy2 = new CharacterEnemy(CharacterClass.Tank, 5);
            enemy2.CreateNode(scene, enemySprite, new Vector2(-8, -2));
            AddCharacter(enemy2);
        }

        private void CreateMap()
        {
            /*
            TmxFile2D mapFile = ResourceCache.GetTmxFile2D("map/levels/starter.tmx");
            if (mapFile == null)
                throw new Exception("Map not found");
                */

            //Node mapNode = scene.CreateChild("TileMap");
            //TileMap2D tileMap = mapNode.CreateComponent<TileMap2D>();
            //tileMap.TmxFile = mapFile;

            Sprite2D groundSprite = ResourceCache.GetSprite2D("map/levels/platformer-art-complete-pack-0/Base pack/Tiles/grassMid.png");
            if (groundSprite == null)
                throw new Exception("Texture not found");

            for (int i = 0; i < 21; ++i)
            {
                MapTile tile = new MapTile(scene, groundSprite, new Vector2(i - 10, -3));
                Tiles.Add(tile);
                collisionObjects.Add(tile);
            }

            for (int i = 0; i < 5; ++i)
            {
                MapTile tile = new MapTile(scene, groundSprite, new Vector2(-10, i - 2));
                Tiles.Add(tile);
                collisionObjects.Add(tile);

                MapTile tile2 = new MapTile(scene, groundSprite, new Vector2(10, i - 2));
                Tiles.Add(tile2);
                collisionObjects.Add(tile2);
            }

            for (int i = 0; i < 4; ++i)
            {
                MapTile tile = new MapTile(scene, groundSprite, new Vector2(i - 5, -1));
                Tiles.Add(tile);
                collisionObjects.Add(tile);
            }

            {
                MapTile tile = new MapTile(scene, groundSprite, new Vector2(2, -2));
                Tiles.Add(tile);
                collisionObjects.Add(tile);
            }

            if (!continueGame) CreatePickups();
        }

        private void CreatePickups()
        {
            var weaponSprite = ResourceCache.GetSprite2D("map/levels/platformer-art-complete-pack-0/Request pack/Tiles/raygunBig.png");
            var armorSprite = ResourceCache.GetSprite2D("map/levels/platformer-art-complete-pack-0/Request pack/Tiles/shieldGold.png");

            if (weaponSprite == null || armorSprite == null)
                throw new Exception("Texture not found");

            for (int i = 0; i < (hardcore ? 2 : 4); ++i)
            {
                Pickups.Add(new PickupWeaponUpgrade(scene, weaponSprite, new Vector2(i - 5, 0)));
            }

            for (int i = 0; i < (hardcore ? 2 : 4); ++i)
            {
                Pickups.Add(new PickupArmor(scene, armorSprite, new Vector2(i - 5, -2)));
            }
        }

        protected async override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            // Pickups
            foreach (Character c in Characters)
            {
                foreach (Pickup p in Pickups.ToList())
                {
                    if (c.Collides(p))
                    {
                        if (p.PickUp(c))
                        {
                            p.WorldNode.Remove();
                            Pickups.Remove(p);
                        }
                    }
                }
            }

            // Bullets
            foreach (Bullet b in Bullets.ToList())
            {
                if(b.WorldNode.Position2D.Length > 50f)
                {
                    b.WorldNode.Remove();
                    Bullets.Remove(b);
                    continue;
                }

                bool deleted = false;
                b.WorldNode.SetPosition2D(b.WorldNode.Position2D + (b.Direction * bulletSpeed * timeStep));

                foreach (Character c in Characters)
                {
                    if (b.Owner == c) continue;
                    if (c.Collides(b))
                    {
                        c.Hit(b);
                        b.WorldNode.Remove();
                        Bullets.Remove(b);
                        deleted = true;
                        break;
                    }
                }

                if (deleted) continue;

                foreach(WorldObject o in collisionObjects)
                {
                    if (o.Collides(b))
                    {
                        b.WorldNode.Remove();
                        Bullets.Remove(b);
                        break;
                    }
                }
            }

            PlayerCharacter.Input.W = Input.GetKeyDown(Key.W);
            PlayerCharacter.Input.A = Input.GetKeyDown(Key.A);
            PlayerCharacter.Input.S = Input.GetKeyDown(Key.S);
            PlayerCharacter.Input.D = Input.GetKeyDown(Key.D);
            PlayerCharacter.Input.Space = Input.GetKeyPress(Key.Space);
            PlayerCharacter.Input.LeftClick = Input.GetKeyDown(Key.E);

            Vector2 mousePosition = new Vector2(Input.MousePosition.X, Input.MousePosition.Y);
            Vector2 resolution = new Vector2(Graphics.Width, Graphics.Height);
            Vector2 mouseUV = ((2f * mousePosition) - resolution) / resolution.Y;
            mouseUV.Y *= -1f;
            PlayerCharacter.Input.MousePosition = mouseUV;

            foreach(Character c in Characters.ToList())
            {
                if(c.Health <= 0)
                {
                    c.WorldNode.Remove();
                    Characters.Remove(c);
                    continue;
                }

                c.UpdateCollision(collisionObjects);
                c.Update(timeStep);
            }

            PlayerCharacter.Input.LeftClick = false;

            if (Input.GetKeyDown(Key.F1))
            {
                Save("latest.txt");
                var saved = new Text() { Value = "Game Saved" };

                saved.SetColor(Color.Cyan);
                saved.SetFont(font: ResourceCache.GetFont("fonts/FiraSans-Regular.otf"), size: 15);
                saved.VerticalAlignment = VerticalAlignment.Center;
                saved.HorizontalAlignment = HorizontalAlignment.Center;

                InvokeOnMain(() => { UI.Root.AddChild(saved); });
                await Task.Delay(500);
                try
                {
                    InvokeOnMain(() =>
                    {
                        try { UI.Root.RemoveChild(saved); }
                        catch { return; }
                    });
                } catch { return; }
            }

            if (Input.GetKeyDown(Key.F2)) {
                timer.Enabled = false;
                Restart();
            }
        }

        public void AddPlayer(CharacterPlayer character)
        {
            PlayerCharacter = character;
            AddCharacter(character);

            cameraNode.Parent = character.WorldNode;
        }

        public void AddCharacter(Character character)
        {
            character.UpgradeWeapon(); // TEMP
            Characters.Add(character);
        }

        private void CreateHUD()
        {
            hud = new UIElement()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                LayoutMode = LayoutMode.Vertical,
                LayoutSpacing = 5
            };

            UpdateHUD();
            UI.Root.AddChild(hud);
        }

        private void CreateClock()
        {
            timer = new Timer(100);
            timer.Elapsed += GameTick;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        // Run every 1/10 second
        private void GameTick(Object source, ElapsedEventArgs e)
        {
            --time;
            UpdateHUD();
        }

        private void UpdateHUD()
        {
            InvokeOnMain(() =>
            {
                hud.RemoveAllChildren();

                var difficulty = new Text() { Value = hardcore ? "Difficulty: Hardcore" : "Difficulty: Normal" };
                var weapon = new Text() { Value = $"Weapon: {PlayerCharacter.HeldWeapon.Serialize()}" };
                var armor = new Text() { Value = PlayerCharacter.Armor ? "Armor: Protected" : "Armor: Missing" };
                var health = new Text() { Value = $"Health: {PlayerCharacter.Health.ToString()}" };
                var clock = new Text() { Value = $"Time: {TimeSpan.FromSeconds(time / 10).ToString(@"mm\:ss")}" };

                difficulty.SetColor(Color.Yellow);
                weapon.SetColor(Color.Yellow);
                armor.SetColor(Color.Yellow);
                health.SetColor(Color.Yellow);
                clock.SetColor(Color.Yellow);

                difficulty.SetFont(font: ResourceCache.GetFont("fonts/FiraSans-Regular.otf"), size: 15);
                weapon.SetFont(font: ResourceCache.GetFont("fonts/FiraSans-Regular.otf"), size: 15);
                armor.SetFont(font: ResourceCache.GetFont("fonts/FiraSans-Regular.otf"), size: 15);
                health.SetFont(font: ResourceCache.GetFont("fonts/FiraSans-Regular.otf"), size: 15);
                clock.SetFont(font: ResourceCache.GetFont("fonts/FiraSans-Regular.otf"), size: 15);

                hud.AddChild(difficulty);
                hud.AddChild(weapon);
                hud.AddChild(armor);
                hud.AddChild(health);
                hud.AddChild(clock);
            });
        }

        public void CreateBullets(List<Bullet> bullets, Character character)
        {
            foreach (Bullet b in bullets)
            {
                b.Owner = character;
                b.CreateNode(scene, bulletSprite, character.WorldNode.Position2D);

                Bullets.Add(b);
            }
        }

        public string Serialize()
        {
            string output = "";

            // Add Player
            output += PlayerCharacter.Serialize();

            // Add Difficulty
            output += Environment.NewLine + hardcore.ToString();

            // Add Time
            output += Environment.NewLine + time.ToString();

            // Add Health
            output += Environment.NewLine + PlayerCharacter.Health.ToString();

            // Add Armor
            output += Environment.NewLine + PlayerCharacter.Armor.ToString();

            // Add enemies
            string characterString = "";
            foreach (var character in Characters.Skip(1)) { characterString += $"{character.Serialize()};"; }
            output += Environment.NewLine + characterString;

            // Add pickups
            string pickupString = "";
            foreach (var item in Pickups) { pickupString += $"{item.Serialize()};"; }
            output += Environment.NewLine + pickupString;

            // Add Weapon
            output += Environment.NewLine + PlayerCharacter.HeldWeapon.Serialize();

            return output;
        }

        public void Deserialize(string serialized)
        {
            using (StringReader reader = new StringReader(serialized))
            {
                string line;
                int lineNumber = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    switch(lineNumber)
                    {
                        case 0: // Player
                            PlayerCharacter.WorldNode.Position = PlayerCharacter.Deserialize(line);
                            ++lineNumber;
                            break;
                        case 1: // Difficulty
                            hardcore = line == "True" ? true : false;
                            ++lineNumber;
                            break;
                        case 2: // Time
                            time = Convert.ToInt32(line);
                            ++lineNumber;
                            break;
                        case 3: // Health
                            PlayerCharacter.Health = Convert.ToInt32(line);
                            ++lineNumber;
                            break;
                        case 4: // Armor
                            PlayerCharacter.Armor = line == "True" ? true : false;
                            ++lineNumber;
                            break;
                        case 5: // Enemies
                            LoadEnemies(line);
                            ++lineNumber;
                            break;
                        case 6: // Pickups
                            LoadPickups(line);
                            ++lineNumber;
                            break;
                        case 7: // Weapon
                            PlayerCharacter.HeldWeapon = Weapon.GetWeaponType(line);
                            ++lineNumber;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void LoadPickups(string line)
        {
            var weaponSprite = ResourceCache.GetSprite2D("map/levels/platformer-art-complete-pack-0/Request pack/Tiles/raygunBig.png");
            var armorSprite = ResourceCache.GetSprite2D("map/levels/platformer-art-complete-pack-0/Request pack/Tiles/shieldGold.png");

            if (weaponSprite == null || armorSprite == null)
                throw new Exception("Texture not found");

            string[] pickupsSplit = line.Split(';');
            for (int i = 0; i < pickupsSplit.Length - 1; i++)
            {
                var pickup = new PickupArmor();
                var position = pickup.Deserialize(pickupsSplit[i]);
                Pickups.Add(new PickupWeaponUpgrade(scene, weaponSprite, new Vector2(position.X, position.Y)));
            }
        }

        private void LoadEnemies(string line)
        {
            var enemySprite = ResourceCache.GetSprite2D("characters/special forces/png2/attack/2_Special_forces_attack_Attack_000.png");
            if (enemySprite == null)
                throw new Exception("Enemy sprite not found");

            string[] enemiesSplit = line.Split(';');
            for (int i = 0; i < enemiesSplit.Length - 1; i++)
            {
                var enemyCharacter = new CharacterEnemy(CharacterClass.Gunner, 5);
                var position = enemyCharacter.Deserialize(enemiesSplit[i]);
                enemyCharacter = new CharacterEnemy(CharacterClass.Gunner, 5, position);
                enemyCharacter.CreateNode(scene, enemySprite, new Vector2(enemyCharacter.Position.X, enemyCharacter.Position.Y));
                AddCharacter(enemyCharacter);
            }   
        }

        public void Load(string fileName)
        {
            string PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);

            if (File.Exists(PATH))
            {
                string data = "";
                foreach (var line in File.ReadLines(PATH)) { data += line + Environment.NewLine; };                
                Deserialize(data);
            }
            else
            {
                throw new Exception("The call could not be completed as dialed. Please check check the number, and try your call again.");
            }
        }

        public void Save(string fileName)
        {
            string PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);

            string serialized = Serialize();
            File.WriteAllText(PATH, serialized);
        }
    }
}
