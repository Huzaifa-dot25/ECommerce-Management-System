using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using E_com.Models;

namespace E_com.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager  = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context      = serviceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.EnsureCreatedAsync();

            // ── ROLES ──────────────────────────────────────────────────────────
            foreach (var role in new[] { "Admin", "Customer" })
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            // ── ADMIN USER ─────────────────────────────────────────────────────
            const string adminEmail = "admin@ecom.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail, Email = adminEmail,
                    FullName = "Administrator", EmailConfirmed = true, Status = true
                };
                var result = await userManager.CreateAsync(admin, "AdminPassword123!");
                if (result.Succeeded) await userManager.AddToRoleAsync(admin, "Admin");
            }

            // ── TEST CUSTOMER ──────────────────────────────────────────────────
            const string customerEmail = "customer@ecom.com";
            if (await userManager.FindByEmailAsync(customerEmail) == null)
            {
                var customer = new ApplicationUser
                {
                    UserName = customerEmail, Email = customerEmail,
                    FullName = "Jane Smith", EmailConfirmed = true, Status = true
                };
                var result = await userManager.CreateAsync(customer, "Customer123!");
                if (result.Succeeded) await userManager.AddToRoleAsync(customer, "Customer");
            }

            // Skip if already seeded
            if (await context.Categories.AnyAsync()) return;

            // ── CATEGORIES ─────────────────────────────────────────────────────
            var categories = new List<Category>
            {
                new() { Name = "Men's Clothing",    Description = "Premium men's fashion — shirts, trousers, jackets and more.", ImagePath = "https://images.unsplash.com/photo-1617137968427-85924c800a22?w=400&q=80", Status = true },
                new() { Name = "Women's Clothing",  Description = "Elegant women's collection — dresses, blouses, co-ords and more.", ImagePath = "https://images.unsplash.com/photo-1567401893414-76b7b1e5a7a5?w=400&q=80", Status = true },
                new() { Name = "Footwear",          Description = "Shoes, sneakers, boots and sandals for every occasion.", ImagePath = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400&q=80", Status = true },
                new() { Name = "Accessories",       Description = "Bags, wallets, belts, watches and jewellery.", ImagePath = "https://images.unsplash.com/photo-1590874103328-eac38a683ce7?w=400&q=80", Status = true },
                new() { Name = "Electronics",       Description = "Headphones, chargers, smartwatches and tech essentials.", ImagePath = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400&q=80", Status = true },
                new() { Name = "Home & Living",     Description = "Décor, lighting, cushions and lifestyle essentials.", ImagePath = "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400&q=80", Status = true },
            };
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            // helper index
            var cat = categories.ToDictionary(c => c.Name, c => c.Id);

            // ── PRODUCTS ───────────────────────────────────────────────────────
            var products = new List<Product>
            {
                // ── Men's Clothing ─────────────────────────────────────────────
                new()
                {
                    Name = "Oxford Button-Down Shirt", SKU = "MEN-SHT-001",
                    CategoryId = cat["Men's Clothing"], Brand = "Polo Heritage",
                    Description = "A timeless slim-fit Oxford shirt woven from 100% combed cotton. Features a button-down collar, chest pocket, and a clean finish that works from desk to dinner.",
                    Price = 89.99m, Discount = 0m, StockQuantity = 45, Status = true
                },
                new()
                {
                    Name = "Slim-Fit Chino Trousers", SKU = "MEN-TRS-002",
                    CategoryId = cat["Men's Clothing"], Brand = "Polo Heritage",
                    Description = "Versatile slim-fit chinos crafted from stretch-cotton blend. Mid-rise waist, flat front, and a tapered leg — perfect for casual Fridays or weekend outings.",
                    Price = 119.00m, Discount = 15m, StockQuantity = 30, Status = true
                },
                new()
                {
                    Name = "Merino Wool V-Neck Sweater", SKU = "MEN-SWT-003",
                    CategoryId = cat["Men's Clothing"], Brand = "Nordvik",
                    Description = "Luxuriously soft extra-fine merino wool. Ribbed cuffs, hem and collar. Breathable yet warm — an essential layering piece for every season.",
                    Price = 145.00m, Discount = 0m, StockQuantity = 22, Status = true
                },
                new()
                {
                    Name = "Quilted Bomber Jacket", SKU = "MEN-JKT-004",
                    CategoryId = cat["Men's Clothing"], Brand = "Arktis",
                    Description = "Water-resistant nylon shell with a lightweight quilted lining. Two zip pockets, ribbed cuffs and a snap-button collar. Warmth without the bulk.",
                    Price = 210.00m, Discount = 20m, StockQuantity = 18, Status = true
                },
                new()
                {
                    Name = "Linen Resort Shirt", SKU = "MEN-SHT-005",
                    CategoryId = cat["Men's Clothing"], Brand = "Soleil",
                    Description = "Relaxed-fit short-sleeve shirt in breathable 100% linen. Notch collar and chest patch pocket. Ideal for warm evenings.",
                    Price = 75.00m, Discount = 10m, StockQuantity = 50, Status = true
                },

                // ── Women's Clothing ───────────────────────────────────────────
                new()
                {
                    Name = "Wrap Midi Dress", SKU = "WOM-DRS-001",
                    CategoryId = cat["Women's Clothing"], Brand = "Élise Paris",
                    Description = "A flattering wrap-style midi dress in floral viscose. Adjustable tie waist, flutter sleeves and a floaty skirt — effortlessly chic for any occasion.",
                    Price = 135.00m, Discount = 0m, StockQuantity = 35, Status = true
                },
                new()
                {
                    Name = "High-Waist Wide-Leg Trousers", SKU = "WOM-TRS-002",
                    CategoryId = cat["Women's Clothing"], Brand = "Élise Paris",
                    Description = "Tailored wide-leg trousers in a luxe crepe fabric. High-rise waist, side zip, and a dramatic flared leg that elongates the silhouette.",
                    Price = 115.00m, Discount = 10m, StockQuantity = 28, Status = true
                },
                new()
                {
                    Name = "Cashmere Turtleneck", SKU = "WOM-KNT-003",
                    CategoryId = cat["Women's Clothing"], Brand = "Nordvik",
                    Description = "Pure Grade-A cashmere in a relaxed ribbed-knit construction. Oversized turtleneck, drop shoulders — the ultimate cosy-luxe statement piece.",
                    Price = 220.00m, Discount = 0m, StockQuantity = 15, Status = true
                },
                new()
                {
                    Name = "Blazer Co-ord Set", SKU = "WOM-SET-004",
                    CategoryId = cat["Women's Clothing"], Brand = "Forma",
                    Description = "Sharp double-breasted blazer paired with matching tailored shorts. Fully lined, padded shoulders. Wear as a set or style separately.",
                    Price = 195.00m, Discount = 25m, StockQuantity = 20, Status = true
                },
                new()
                {
                    Name = "Satin Slip Dress", SKU = "WOM-DRS-005",
                    CategoryId = cat["Women's Clothing"], Brand = "Velour",
                    Description = "Bias-cut satin slip dress with delicate lace trim at neckline and hem. Adjustable spaghetti straps and a subtle side slit for graceful movement.",
                    Price = 160.00m, Discount = 15m, StockQuantity = 25, Status = true
                },

                // ── Footwear ───────────────────────────────────────────────────
                new()
                {
                    Name = "Classic White Leather Sneaker", SKU = "FTW-SNK-001",
                    CategoryId = cat["Footwear"], Brand = "Stride",
                    Description = "Minimalist low-top sneaker in full-grain white leather. Cushioned insole, vulcanised rubber sole. Goes with everything — jeans, chinos, dresses.",
                    Price = 130.00m, Discount = 0m, StockQuantity = 60, Status = true
                },
                new()
                {
                    Name = "Suede Chelsea Boot", SKU = "FTW-BOT-002",
                    CategoryId = cat["Footwear"], Brand = "Brixton",
                    Description = "Slip-on Chelsea boot in premium sand suede. Elasticated side panels, stacked leather heel, and a leather-lined interior for all-day comfort.",
                    Price = 185.00m, Discount = 10m, StockQuantity = 40, Status = true
                },
                new()
                {
                    Name = "Running Pro Trainer", SKU = "FTW-TRN-003",
                    CategoryId = cat["Footwear"], Brand = "KineticX",
                    Description = "Engineered mesh upper with a responsive foam midsole and carbon-fibre reinforced outsole. Lightweight at 240g. Built for pace.",
                    Price = 165.00m, Discount = 20m, StockQuantity = 55, Status = true
                },
                new()
                {
                    Name = "Block-Heel Mule", SKU = "FTW-MUL-004",
                    CategoryId = cat["Footwear"], Brand = "Soleil",
                    Description = "Open-toe mule in polished croc-embossed leather. 5 cm block heel for stability. A sophisticated everyday staple.",
                    Price = 140.00m, Discount = 0m, StockQuantity = 30, Status = true
                },
                new()
                {
                    Name = "Hiker Lace-Up Boot", SKU = "FTW-HIK-005",
                    CategoryId = cat["Footwear"], Brand = "Brixton",
                    Description = "Full-grain leather upper with a waterproof lining and Vibram outsole. Padded ankle collar and lace-up closure for a secure fit on any terrain.",
                    Price = 230.00m, Discount = 0m, StockQuantity = 20, Status = true
                },

                // ── Accessories ────────────────────────────────────────────────
                new()
                {
                    Name = "Pebbled Leather Tote Bag", SKU = "ACC-BAG-001",
                    CategoryId = cat["Accessories"], Brand = "Maison K",
                    Description = "Structured tote in pebbled Italian leather. Interior zip pocket, two open pockets, and an adjustable crossbody strap. Fits a 13\" laptop.",
                    Price = 275.00m, Discount = 0m, StockQuantity = 18, Status = true
                },
                new()
                {
                    Name = "Minimalist Leather Wallet", SKU = "ACC-WAL-002",
                    CategoryId = cat["Accessories"], Brand = "Maison K",
                    Description = "Slim bifold wallet in full-grain cowhide. Six card slots, two bill compartments, and an RFID-blocking lining. Gets better with age.",
                    Price = 85.00m, Discount = 0m, StockQuantity = 70, Status = true
                },
                new()
                {
                    Name = "Woven Canvas Belt", SKU = "ACC-BLT-003",
                    CategoryId = cat["Accessories"], Brand = "Nordvik",
                    Description = "Reversible woven canvas belt with a polished brass pin buckle. Navy/tan reversible strap — two belts in one.",
                    Price = 55.00m, Discount = 10m, StockQuantity = 90, Status = true
                },
                new()
                {
                    Name = "Aviator Sunglasses", SKU = "ACC-SNG-004",
                    CategoryId = cat["Accessories"], Brand = "Lumière",
                    Description = "Classic teardrop aviator frames in brushed gold metal. Polarised UV400 mineral glass lenses. Includes a hard case and cleaning cloth.",
                    Price = 120.00m, Discount = 15m, StockQuantity = 40, Status = true
                },
                new()
                {
                    Name = "Silk Pocket Square", SKU = "ACC-PKT-005",
                    CategoryId = cat["Accessories"], Brand = "Élise Paris",
                    Description = "Hand-rolled 100% silk pocket square in a garden floral print. 30×30 cm. Adds a refined finishing touch to any tailored look.",
                    Price = 45.00m, Discount = 0m, StockQuantity = 100, Status = true
                },

                // ── Electronics ────────────────────────────────────────────────
                new()
                {
                    Name = "Wireless Noise-Cancelling Headphones", SKU = "ELC-HDP-001",
                    CategoryId = cat["Electronics"], Brand = "SoundCore",
                    Description = "40-hour battery life, industry-leading ANC, and foldable design. Hi-Res Audio certified. USB-C fast charge — 15 min = 3 hrs playback.",
                    Price = 299.00m, Discount = 15m, StockQuantity = 30, Status = true
                },
                new()
                {
                    Name = "True Wireless Earbuds", SKU = "ELC-EBD-002",
                    CategoryId = cat["Electronics"], Brand = "SoundCore",
                    Description = "6-mm dynamic drivers, IPX5 sweat resistance, 7-hr bud / 28-hr case battery. Adaptive EQ and multipoint Bluetooth 5.3.",
                    Price = 149.00m, Discount = 10m, StockQuantity = 50, Status = true
                },
                new()
                {
                    Name = "Smart Watch Series 5", SKU = "ELC-WTC-003",
                    CategoryId = cat["Electronics"], Brand = "OmniTime",
                    Description = "AMOLED always-on display, GPS, heart-rate, SpO2, sleep tracking. 14-day battery. Aluminium case, interchangeable straps. iOS & Android.",
                    Price = 349.00m, Discount = 0m, StockQuantity = 25, Status = true
                },
                new()
                {
                    Name = "65W GaN USB-C Charger", SKU = "ELC-CHG-004",
                    CategoryId = cat["Electronics"], Brand = "PowerFlow",
                    Description = "3-port GaN charger — 2× USB-C (65W+20W) + 1× USB-A (18W). Charges a laptop, phone and earbuds simultaneously. Compact travel size.",
                    Price = 59.00m, Discount = 0m, StockQuantity = 120, Status = true
                },
                new()
                {
                    Name = "Portable Bluetooth Speaker", SKU = "ELC-SPK-005",
                    CategoryId = cat["Electronics"], Brand = "SoundCore",
                    Description = "360° sound, IPX7 waterproof, 20-hr battery, built-in power bank. Dual passive radiators for deep bass. Pairs two speakers in stereo.",
                    Price = 119.00m, Discount = 20m, StockQuantity = 40, Status = true
                },

                // ── Home & Living ──────────────────────────────────────────────
                new()
                {
                    Name = "Linen Throw Cushion Set (2)", SKU = "HOM-CSH-001",
                    CategoryId = cat["Home & Living"], Brand = "Maison Douce",
                    Description = "Set of two 50×50 cm cushion covers in stonewashed linen. Hidden zip closure. Machine washable. Available in sage, sand and clay.",
                    Price = 65.00m, Discount = 0m, StockQuantity = 60, Status = true
                },
                new()
                {
                    Name = "Scented Soy Candle — Cedarwood & Amber", SKU = "HOM-CND-002",
                    CategoryId = cat["Home & Living"], Brand = "Maison Douce",
                    Description = "Hand-poured 300g soy wax candle with a cotton wick. 60-hour burn time. Notes of warm cedarwood, amber resin and a hint of vanilla.",
                    Price = 42.00m, Discount = 0m, StockQuantity = 80, Status = true
                },
                new()
                {
                    Name = "Ceramic Pour-Over Coffee Set", SKU = "HOM-COF-003",
                    CategoryId = cat["Home & Living"], Brand = "Brewcraft",
                    Description = "Hand-thrown stoneware dripper with matching carafe and two mugs. Includes reusable stainless filter. Makes 2–3 cups. Dishwasher safe.",
                    Price = 98.00m, Discount = 10m, StockQuantity = 35, Status = true
                },
                new()
                {
                    Name = "Bamboo Desk Organiser", SKU = "HOM-DSK-004",
                    CategoryId = cat["Home & Living"], Brand = "Greenform",
                    Description = "Six-compartment bamboo tray for pens, cards, scissors and stationery. Natural finish, eco-sourced sustainable bamboo. 30 × 20 × 8 cm.",
                    Price = 38.00m, Discount = 0m, StockQuantity = 75, Status = true
                },
                new()
                {
                    Name = "Moroccan Wool Throw Blanket", SKU = "HOM-THR-005",
                    CategoryId = cat["Home & Living"], Brand = "Maison Douce",
                    Description = "Handwoven in Morocco from virgin wool with a fringed border. Striped geometric pattern. 130 × 180 cm. Dry clean only.",
                    Price = 125.00m, Discount = 30m, StockQuantity = 22, Status = true
                },
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            // ── PRODUCT IMAGES (Unsplash) ──────────────────────────────────────
            // Uses product index within the list (0-based)
            var images = new List<ProductImage>
            {
                // Men's Clothing
                new() { ProductId = products[0].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1598522325074-042db73aa4e6?w=600&q=80" },
                new() { ProductId = products[0].Id, IsPrimary = false, ImagePath = "https://images.unsplash.com/photo-1562157873-818bc0726f68?w=600&q=80" },
                new() { ProductId = products[1].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1541099649105-f69ad21f3246?w=600&q=80" },
                new() { ProductId = products[1].Id, IsPrimary = false, ImagePath = "https://images.unsplash.com/photo-1473966968600-fa801b869a1a?w=600&q=80" },
                new() { ProductId = products[2].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1614975059251-992f11792b9f?w=600&q=80" },
                new() { ProductId = products[3].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1591047139829-d91aecb6caea?w=600&q=80" },
                new() { ProductId = products[4].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1564584217132-2271feaeb3c5?w=600&q=80" },

                // Women's Clothing
                new() { ProductId = products[5].Id,  IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1572804013309-59a88b7e92f1?w=600&q=80" },
                new() { ProductId = products[5].Id,  IsPrimary = false, ImagePath = "https://images.unsplash.com/photo-1485968579580-b6d095142e6e?w=600&q=80" },
                new() { ProductId = products[6].Id,  IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1509631179647-0177331693ae?w=600&q=80" },
                new() { ProductId = products[7].Id,  IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1608234808654-2a8875faa7fd?w=600&q=80" },
                new() { ProductId = products[8].Id,  IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1548624313-0396a61b0dc5?w=600&q=80" },
                new() { ProductId = products[9].Id,  IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1596944924591-6a3b5c2b7ec1?w=600&q=80" },

                // Footwear
                new() { ProductId = products[10].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=600&q=80" },
                new() { ProductId = products[10].Id, IsPrimary = false, ImagePath = "https://images.unsplash.com/photo-1460353581641-37baddab0fa2?w=600&q=80" },
                new() { ProductId = products[11].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1605812860427-4024433a70fd?w=600&q=80" },
                new() { ProductId = products[12].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1606107557195-0e29a4b5b4aa?w=600&q=80" },
                new() { ProductId = products[13].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1543163521-1bf539c55dd2?w=600&q=80" },
                new() { ProductId = products[14].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1638247025967-b4e38f787b76?w=600&q=80" },

                // Accessories
                new() { ProductId = products[15].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1548036328-c9fa89d128fa?w=600&q=80" },
                new() { ProductId = products[16].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1627123424574-724758594e93?w=600&q=80" },
                new() { ProductId = products[17].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=600&q=80" },
                new() { ProductId = products[18].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1511499767150-a48a237f0083?w=600&q=80" },
                new() { ProductId = products[19].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?w=600&q=80" },

                // Electronics
                new() { ProductId = products[20].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&q=80" },
                new() { ProductId = products[20].Id, IsPrimary = false, ImagePath = "https://images.unsplash.com/photo-1583394838336-acd977736f90?w=600&q=80" },
                new() { ProductId = products[21].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1590658268037-6bf12165a8df?w=600&q=80" },
                new() { ProductId = products[22].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600&q=80" },
                new() { ProductId = products[23].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1563013544-824ae1b704d3?w=600&q=80" },
                new() { ProductId = products[24].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=600&q=80" },

                // Home & Living
                new() { ProductId = products[25].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=600&q=80" },
                new() { ProductId = products[26].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1602143407151-7111542de6e8?w=600&q=80" },
                new() { ProductId = products[27].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=600&q=80" },
                new() { ProductId = products[28].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1593078166039-c9878df5c520?w=600&q=80" },
                new() { ProductId = products[29].Id, IsPrimary = true,  ImagePath = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=600&q=80" },
            };

            await context.ProductImages.AddRangeAsync(images);
            await context.SaveChangesAsync();

            // ── COUPONS ────────────────────────────────────────────────────────
            var coupons = new List<Coupon>
            {
                new() { CouponCode = "WELCOME10", DiscountPercentage = 10m, ExpiryDate = DateTime.UtcNow.AddMonths(12), Status = true },
                new() { CouponCode = "SAVE20",    DiscountPercentage = 20m, ExpiryDate = DateTime.UtcNow.AddMonths(6),  Status = true },
                new() { CouponCode = "FLASH30",   DiscountPercentage = 30m, ExpiryDate = DateTime.UtcNow.AddDays(30),   Status = true },
                new() { CouponCode = "VIP40",     DiscountPercentage = 40m, ExpiryDate = DateTime.UtcNow.AddMonths(3),  Status = true },
                new() { CouponCode = "SUMMER15",  DiscountPercentage = 15m, ExpiryDate = DateTime.UtcNow.AddMonths(2),  Status = true },
            };

            await context.Coupons.AddRangeAsync(coupons);
            await context.SaveChangesAsync();
        }
    }
}
