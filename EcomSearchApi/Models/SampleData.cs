namespace EcomSearchApi.Models;

public static class SampleData
{
    public static readonly List<Product> InitialProducts =
    [
        new()
        {
            Id = 1,
            Name = "Sony WH-1000XM5 Wireless Headphones",
            Description = "Industry leading noise canceling wireless over-ear headphones with superior audio clarity and 30 hours battery life.",
            Category = "Electronics",
            Brand = "Sony",
            Price = 399.99m,
            Stock = 45,
            Tags = ["audio", "wireless", "noise-canceling", "premium"],
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        },
        new()
        {
            Id = 2,
            Name = "Apple MacBook Pro 16\" M3 Max",
            Description = "Powerful laptop engineered for extreme workflows with 36GB unified memory, liquid retina XDR display and supreme battery efficiency.",
            Category = "Computers",
            Brand = "Apple",
            Price = 3499.00m,
            Stock = 12,
            Tags = ["laptop", "m3", "apple", "premium", "workstation"],
            CreatedAt = DateTime.UtcNow.AddDays(-25)
        },
        new()
        {
            Id = 3,
            Name = "Samsung Galaxy S24 Ultra 512GB",
            Description = "AI powered flagship smartphone with integrated S-Pen, quad telephoto camera, titanium frame and Snapdragon 8 Gen 3.",
            Category = "Smartphones",
            Brand = "Samsung",
            Price = 1299.99m,
            Stock = 80,
            Tags = ["smartphone", "5g", "ai", "android", "flagship"],
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        },
        new()
        {
            Id = 4,
            Name = "Logitech MX Master 3S Wireless Mouse",
            Description = "Ergonomic quiet performance mouse with 8K DPI sensor, magspeed electromagnetic scrolling, and multi-device flow.",
            Category = "Accessories",
            Brand = "Logitech",
            Price = 99.99m,
            Stock = 120,
            Tags = ["mouse", "ergonomic", "wireless", "productivity"],
            CreatedAt = DateTime.UtcNow.AddDays(-18)
        },
        new()
        {
            Id = 5,
            Name = "Dyson V15 Detect Cordless Vacuum",
            Description = "Intelligent cordless stick vacuum cleaner with laser illumination revealing microscopic dust and powerful suction.",
            Category = "Home Appliances",
            Brand = "Dyson",
            Price = 749.99m,
            Stock = 25,
            Tags = ["cleaning", "cordless", "smart-home", "premium"],
            CreatedAt = DateTime.UtcNow.AddDays(-15)
        },
        new()
        {
            Id = 6,
            Name = "Nike Air Zoom Pegasus 40 Running Shoes",
            Description = "Responsive everyday running shoes featuring dual Zoom Air units and engineered mesh for maximum comfort and durability.",
            Category = "Footwear",
            Brand = "Nike",
            Price = 130.00m,
            Stock = 150,
            Tags = ["running", "sports", "sneakers", "fitness"],
            CreatedAt = DateTime.UtcNow.AddDays(-12)
        },
        new()
        {
            Id = 7,
            Name = "Amazon Echo Dot 5th Gen Smart Speaker",
            Description = "Compact smart speaker with vibrant sound, Alexa voice assistant, temperature sensor and smart home hub controls.",
            Category = "Smart Home",
            Brand = "Amazon",
            Price = 49.99m,
            Stock = 300,
            Tags = ["alexa", "smart-speaker", "smart-home", "iot"],
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        },
        new()
        {
            Id = 8,
            Name = "Nespresso VertuoPlus Coffee & Espresso Maker",
            Description = "Single-serve coffee maker with centrifusion extraction technology for freshly brewed rich crema coffee and espresso.",
            Category = "Kitchen",
            Brand = "Nespresso",
            Price = 169.00m,
            Stock = 60,
            Tags = ["coffee", "espresso", "kitchen", "beverage"],
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        },
        new()
        {
            Id = 9,
            Name = "ASUS ROG Swift 27\" 4K Gaming Monitor",
            Description = "Ultrafast 144Hz 4K HDR gaming monitor with G-SYNC compatibility, 1ms response time and Aura Sync RGB lighting.",
            Category = "Monitors",
            Brand = "ASUS",
            Price = 899.99m,
            Stock = 18,
            Tags = ["gaming", "4k", "monitor", "hdr", "rog"],
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        },
        new()
        {
            Id = 10,
            Name = "Anker 737 Power Bank (PowerCore 24K)",
            Description = "Ultra-powerful 140W two-way fast charging portable battery with smart digital display and 24000mAh capacity.",
            Category = "Accessories",
            Brand = "Anker",
            Price = 149.99m,
            Stock = 90,
            Tags = ["power-bank", "charger", "usb-c", "travel"],
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        }
    ];
}
