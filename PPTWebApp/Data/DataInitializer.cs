using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace PPTWebApp.Data
{
    public class DataInitializer
    {
        private readonly string _connectionString;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DataInitializer(string connectionString, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _connectionString = connectionString;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task InitializeDataAsync()
        {
            CreateTables();
            CreateTriggers();
            await CreateUsersAndRoles();
            InsertDummyData(9);
        }

        private void CreateTables()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string createTablesSql = @"
                    CREATE TABLE IF NOT EXISTS aspnetroles (
                        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                        name VARCHAR(256) NOT NULL,
                        normalizedname VARCHAR(256) NOT NULL,
                        concurrencystamp VARCHAR(256)
                    );

                    CREATE TABLE IF NOT EXISTS aspnetusers (
                        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                        username VARCHAR(256) NOT NULL,
                        normalizedusername VARCHAR(256) NOT NULL,
                        email VARCHAR(256),
                        normalizedemail VARCHAR(256),
                        emailconfirmed BOOLEAN NOT NULL,
                        passwordhash TEXT,
                        securitystamp TEXT,
                        concurrencystamp TEXT,
                        phonenumber TEXT,
                        phonenumberconfirmed BOOLEAN NOT NULL,
                        twofactorenabled BOOLEAN NOT NULL,
                        lockoutend TIMESTAMP,
                        lockoutenabled BOOLEAN NOT NULL,
                        accessfailedcount INTEGER NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS aspnetuserroles (
                        userid UUID NOT NULL,
                        roleid UUID NOT NULL,
                        PRIMARY KEY (userid, roleid),
                        CONSTRAINT fk_aspnetuserroles_aspnetusers FOREIGN KEY (userid) REFERENCES aspnetusers (id) ON DELETE CASCADE,
                        CONSTRAINT fk_aspnetuserroles_aspnetroles FOREIGN KEY (roleid) REFERENCES aspnetroles (id) ON DELETE CASCADE
                    );

                    CREATE TABLE IF NOT EXISTS aspnetuserclaims (
                        id SERIAL PRIMARY KEY,
                        userid UUID NOT NULL,
                        claimtype TEXT,
                        claimvalue TEXT,
                        CONSTRAINT fk_aspnetuserclaims_aspnetusers FOREIGN KEY (userid) REFERENCES aspnetusers (id) ON DELETE CASCADE
                    );

                    CREATE TABLE IF NOT EXISTS aspnetuserlogins (
                        loginprovider VARCHAR(128) NOT NULL,
                        providerkey VARCHAR(128) NOT NULL,
                        providerdisplayname TEXT,
                        userid UUID NOT NULL,
                        PRIMARY KEY (loginprovider, providerkey),
                        CONSTRAINT fk_aspnetuserlogins_aspnetusers FOREIGN KEY (userid) REFERENCES aspnetusers (id) ON DELETE CASCADE
                    );

                    CREATE TABLE IF NOT EXISTS aspnetusertokens (
                        userid UUID NOT NULL,
                        loginprovider VARCHAR(128) NOT NULL,
                        name VARCHAR(128) NOT NULL,
                        value TEXT,
                        PRIMARY KEY (userid, loginprovider, name),
                        CONSTRAINT fk_aspnetusertokens_aspnetusers FOREIGN KEY (userid) REFERENCES aspnetusers (id) ON DELETE CASCADE
                    );

                    CREATE TABLE IF NOT EXISTS aspnetroleclaims (
                        id SERIAL PRIMARY KEY,
                        roleid UUID NOT NULL,
                        claimtype TEXT,
                        claimvalue TEXT,
                        CONSTRAINT fk_aspnetroleclaims_aspnetroles FOREIGN KEY (roleid) REFERENCES aspnetroles (id) ON DELETE CASCADE
                    );

                    CREATE TABLE IF NOT EXISTS visitorsessions (
                        id SERIAL PRIMARY KEY,
                        sessionid UUID UNIQUE NOT NULL DEFAULT gen_random_uuid(),
                        startedat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        endedat TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS visitorpageviews (
                        id SERIAL PRIMARY KEY,
                        sessionid UUID NOT NULL,
                        pageurl VARCHAR(255) NOT NULL,
                        viewedat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        referrer TEXT,
                        CONSTRAINT fk_visitorpageviews_visitorsessions
                            FOREIGN KEY (sessionid) REFERENCES visitorsessions (sessionid) ON DELETE CASCADE
                    );

                    CREATE TABLE IF NOT EXISTS posts (
                        id SERIAL PRIMARY KEY,
                        title VARCHAR(255) NOT NULL,
                        content TEXT NOT NULL,
                        imageurl VARCHAR(255) NOT NULL,
                        imagecompromise VARCHAR(50) NOT NULL,
                        dateposted TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        createdat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        modifiedat TIMESTAMP,
                        deletedat TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS productcategories (
                        id SERIAL PRIMARY KEY,
                        name VARCHAR(255) NOT NULL,
                        description TEXT NOT NULL,
                        createdat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        modifiedat TIMESTAMP,
                        deletedat TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS productinventories (
                        id SERIAL PRIMARY KEY,
                        quantity INT NOT NULL,
                        createdat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        modifiedat TIMESTAMP,
                        deletedat TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS discounts (
                        id SERIAL PRIMARY KEY,
                        name VARCHAR(255) NOT NULL,
                        description TEXT NOT NULL,
                        discountpercent DECIMAL(5, 2) NOT NULL,
                        active BOOLEAN NOT NULL DEFAULT TRUE,
                        createdat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        modifiedat TIMESTAMP,
                        deletedat TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS products (
                        id SERIAL PRIMARY KEY,
                        name VARCHAR(255) NOT NULL,
                        description TEXT NOT NULL,
                        sku VARCHAR(255) NOT NULL UNIQUE,
                        categoryid INT REFERENCES productcategories(id) ON DELETE SET NULL,
                        inventoryid INT REFERENCES productinventories(id) ON DELETE SET NULL,
                        price DECIMAL(10, 2) NOT NULL,
                        discountid INT REFERENCES discounts(id) ON DELETE SET NULL,
                        imageurl VARCHAR(255) NOT NULL,
                        imagecompromise VARCHAR(50) NOT NULL,
                        createdat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        modifiedat TIMESTAMP,
                        deletedat TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS producthighlights (
                        id SERIAL PRIMARY KEY,
                        productid INT,
                        position INT NOT NULL,
                        createdat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (productid) REFERENCES products(id) ON DELETE CASCADE,
                        UNIQUE (position)
                    );

                    CREATE TABLE IF NOT EXISTS userprofiles (
                        id SERIAL PRIMARY KEY,
                        username VARCHAR(255) NOT NULL,
                        password TEXT NOT NULL,
                        firstname VARCHAR(255),
                        lastname VARCHAR(255),
                        telephone VARCHAR(20),
                        createdat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        modifiedat TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS useractivity (
                        userid UUID NOT NULL,
                        lastactivityat TIMESTAMP NOT NULL,
                        PRIMARY KEY (userid)
                    );

                    CREATE TABLE IF NOT EXISTS useraddresses (
                        id SERIAL PRIMARY KEY,
                        userid INT REFERENCES userprofiles(id) ON DELETE CASCADE,
                        addressline1 VARCHAR(255) NOT NULL,
                        addressline2 VARCHAR(255),
                        city VARCHAR(100),
                        postalcode VARCHAR(20),
                        county VARCHAR(100),
                        telephone VARCHAR(20),
                        mobile VARCHAR(20)
                    );

                    CREATE TABLE IF NOT EXISTS userpayments (
                        id SERIAL PRIMARY KEY,
                        userid INT REFERENCES userprofiles(id) ON DELETE CASCADE,
                        paymenttype VARCHAR(50),
                        provider VARCHAR(255),
                        accountno VARCHAR(50),
                        expiry DATE
                    );

                    CREATE TABLE IF NOT EXISTS orderdetails (
                        id SERIAL PRIMARY KEY,
                        userid INT REFERENCES userprofiles(id) ON DELETE SET NULL,
                        total DECIMAL(10, 2) NOT NULL,
                        paymentid INT REFERENCES userpayments(id) ON DELETE SET NULL,
                        createdat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        modifiedat TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS orderitems (
                        id SERIAL PRIMARY KEY,
                        orderid INT REFERENCES orderdetails(id) ON DELETE CASCADE,
                        productid INT REFERENCES products(id) ON DELETE SET NULL,
                        quantity INT NOT NULL,
                        createdat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        modifiedat TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS paymentdetails (
                        id SERIAL PRIMARY KEY,
                        orderid INT REFERENCES orderdetails(id) ON DELETE CASCADE,
                        amount DECIMAL(10, 2) NOT NULL,
                        provider VARCHAR(255),
                        status VARCHAR(50),
                        createdat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        modifiedat TIMESTAMP
                    );
                ";

                using (var createCommand = new NpgsqlCommand(createTablesSql, connection))
                {
                    createCommand.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        private void CreateTriggers()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string createTriggersSql = @"
                    CREATE OR REPLACE FUNCTION update_modifiedat_posts()
                    RETURNS TRIGGER AS $$
                    BEGIN
                        NEW.modifiedat = CURRENT_TIMESTAMP;
                        RETURN NEW;
                    END;
                    $$ LANGUAGE plpgsql;

                    DO $$
                    BEGIN
                        IF NOT EXISTS (
                            SELECT 1 FROM pg_trigger WHERE tgname = 'update_modifiedat_trigger_posts'
                        ) THEN
                            CREATE TRIGGER update_modifiedat_trigger_posts
                            BEFORE UPDATE ON posts
                            FOR EACH ROW
                            EXECUTE FUNCTION update_modifiedat_posts();
                        END IF;
                    END $$;

                    CREATE OR REPLACE FUNCTION update_modifiedat_productcategories()
                    RETURNS TRIGGER AS $$
                    BEGIN
                        NEW.modifiedat = CURRENT_TIMESTAMP;
                        RETURN NEW;
                    END;
                    $$ LANGUAGE plpgsql;

                    DO $$
                    BEGIN
                        IF NOT EXISTS (
                            SELECT 1 FROM pg_trigger WHERE tgname = 'update_modifiedat_trigger_productcategories'
                        ) THEN
                            CREATE TRIGGER update_modifiedat_trigger_productcategories
                            BEFORE UPDATE ON productcategories
                            FOR EACH ROW
                            EXECUTE FUNCTION update_modifiedat_productcategories();
                        END IF;
                    END $$;

                    CREATE OR REPLACE FUNCTION update_modifiedat_productinventories()
                    RETURNS TRIGGER AS $$
                    BEGIN
                        NEW.modifiedat = CURRENT_TIMESTAMP;
                        RETURN NEW;
                    END;
                    $$ LANGUAGE plpgsql;

                    DO $$
                    BEGIN
                        IF NOT EXISTS (
                            SELECT 1 FROM pg_trigger WHERE tgname = 'update_modifiedat_trigger_productinventories'
                        ) THEN
                            CREATE TRIGGER update_modifiedat_trigger_productinventories
                            BEFORE UPDATE ON productinventories
                            FOR EACH ROW
                            EXECUTE FUNCTION update_modifiedat_productinventories();
                        END IF;
                    END $$;

                    CREATE OR REPLACE FUNCTION update_modifiedat_discounts()
                    RETURNS TRIGGER AS $$
                    BEGIN
                        NEW.modifiedat = CURRENT_TIMESTAMP;
                        RETURN NEW;
                    END;
                    $$ LANGUAGE plpgsql;

                    DO $$
                    BEGIN
                        IF NOT EXISTS (
                            SELECT 1 FROM pg_trigger WHERE tgname = 'update_modifiedat_trigger_discounts'
                        ) THEN
                            CREATE TRIGGER update_modifiedat_trigger_discounts
                            BEFORE UPDATE ON discounts
                            FOR EACH ROW
                            EXECUTE FUNCTION update_modifiedat_discounts();
                        END IF;
                    END $$;

                    CREATE OR REPLACE FUNCTION update_modifiedat_products()
                    RETURNS TRIGGER AS $$
                    BEGIN
                        NEW.modifiedat = CURRENT_TIMESTAMP;
                        RETURN NEW;
                    END;
                    $$ LANGUAGE plpgsql;

                    DO $$
                    BEGIN
                        IF NOT EXISTS (
                            SELECT 1 FROM pg_trigger WHERE tgname = 'update_modifiedat_trigger_products'
                        ) THEN
                            CREATE TRIGGER update_modifiedat_trigger_products
                            BEFORE UPDATE ON products
                            FOR EACH ROW
                            EXECUTE FUNCTION update_modifiedat_products();
                        END IF;
                    END $$;

                    CREATE OR REPLACE FUNCTION update_modifiedat_userprofiles()
                    RETURNS TRIGGER AS $$
                    BEGIN
                        NEW.modifiedat = CURRENT_TIMESTAMP;
                        RETURN NEW;
                    END;
                    $$ LANGUAGE plpgsql;

                    DO $$
                    BEGIN
                        IF NOT EXISTS (
                            SELECT 1 FROM pg_trigger WHERE tgname = 'update_modifiedat_trigger_userprofiles'
                        ) THEN
                            CREATE TRIGGER update_modifiedat_trigger_userprofiles
                            BEFORE UPDATE ON userprofiles
                            FOR EACH ROW
                            EXECUTE FUNCTION update_modifiedat_userprofiles();
                        END IF;
                    END $$;

                    CREATE OR REPLACE FUNCTION update_modifiedat_orderdetails()
                    RETURNS TRIGGER AS $$
                    BEGIN
                        NEW.modifiedat = CURRENT_TIMESTAMP;
                        RETURN NEW;
                    END;
                    $$ LANGUAGE plpgsql;

                    DO $$
                    BEGIN
                        IF NOT EXISTS (
                            SELECT 1 FROM pg_trigger WHERE tgname = 'update_modifiedat_trigger_orderdetails'
                        ) THEN
                            CREATE TRIGGER update_modifiedat_trigger_orderdetails
                            BEFORE UPDATE ON orderdetails
                            FOR EACH ROW
                            EXECUTE FUNCTION update_modifiedat_orderdetails();
                        END IF;
                    END $$;

                    CREATE OR REPLACE FUNCTION update_modifiedat_orderitems()
                    RETURNS TRIGGER AS $$
                    BEGIN
                        NEW.modifiedat = CURRENT_TIMESTAMP;
                        RETURN NEW;
                    END;
                    $$ LANGUAGE plpgsql;

                    DO $$
                    BEGIN
                        IF NOT EXISTS (
                            SELECT 1 FROM pg_trigger WHERE tgname = 'update_modifiedat_trigger_orderitems'
                        ) THEN
                            CREATE TRIGGER update_modifiedat_trigger_orderitems
                            BEFORE UPDATE ON orderitems
                            FOR EACH ROW
                            EXECUTE FUNCTION update_modifiedat_orderitems();
                        END IF;
                    END $$;

                    CREATE OR REPLACE FUNCTION update_modifiedat_paymentdetails()
                    RETURNS TRIGGER AS $$
                    BEGIN
                        NEW.modifiedat = CURRENT_TIMESTAMP;
                        RETURN NEW;
                    END;
                    $$ LANGUAGE plpgsql;

                    DO $$
                    BEGIN
                        IF NOT EXISTS (
                            SELECT 1 FROM pg_trigger WHERE tgname = 'update_modifiedat_trigger_paymentdetails'
                        ) THEN
                            CREATE TRIGGER update_modifiedat_trigger_paymentdetails
                            BEFORE UPDATE ON paymentdetails
                            FOR EACH ROW
                            EXECUTE FUNCTION update_modifiedat_paymentdetails();
                        END IF;
                    END $$;
                    ";

                using (var createCommand = new NpgsqlCommand(createTriggersSql, connection))
                {
                    createCommand.ExecuteNonQuery();
                }

                connection.Close();
            }
        }


        private void InsertDummyData(int count)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string insertCategorySql = @"
                    INSERT INTO productcategories (name, description, createdat)
                    SELECT @Name, @Description, @CreatedAt
                    WHERE NOT EXISTS (SELECT 1 FROM productcategories WHERE name = @Name)";

                // Category 1: Electronics
                using (var insertCategoryCommand = new NpgsqlCommand(insertCategorySql, connection))
                {
                    insertCategoryCommand.Parameters.AddWithValue("@Name", "Electronics");
                    insertCategoryCommand.Parameters.AddWithValue("@Description", "Electronic devices and gadgets.");
                    insertCategoryCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                    insertCategoryCommand.ExecuteNonQuery();
                }

                // Category 2: Software
                using (var insertCategoryCommand = new NpgsqlCommand(insertCategorySql, connection))
                {
                    insertCategoryCommand.Parameters.AddWithValue("@Name", "Software");
                    insertCategoryCommand.Parameters.AddWithValue("@Description", "Software products with digital delivery.");
                    insertCategoryCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                    insertCategoryCommand.ExecuteNonQuery();
                }

                for (int i = 1; i <= count; i++)
                {
                    int categoryId = (i % 2 == 0) ? 2 : 1;
                    int? inventoryId = null;

                    if (categoryId == 1)
                    {
                        string insertInventorySql = @"
                            INSERT INTO productinventories (quantity, createdat)
                            VALUES (@Quantity, @CreatedAt) RETURNING id";

                        using (var insertInventoryCommand = new NpgsqlCommand(insertInventorySql, connection))
                        {
                            insertInventoryCommand.Parameters.AddWithValue("@Quantity", 100 + i);
                            insertInventoryCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                            inventoryId = (int)insertInventoryCommand.ExecuteScalar();
                        }
                    }

                    string insertProductSql = @"
                        INSERT INTO products (name, description, sku, categoryid, inventoryid, price, imageurl, imagecompromise)
                        SELECT @Name, @Description, @SKU, @CategoryId, @InventoryId, @Price, @ImageUrl, @ImageCompromise
                        WHERE NOT EXISTS (SELECT 1 FROM products WHERE name = @Name)";

                    using (var insertProductCommand = new NpgsqlCommand(insertProductSql, connection))
                    {
                        insertProductCommand.Parameters.AddWithValue("@Name", $"Product {i}");
                        insertProductCommand.Parameters.AddWithValue("@Description", $"This is the description of product {i}.");
                        insertProductCommand.Parameters.AddWithValue("@SKU", $"SKU-{i}");
                        insertProductCommand.Parameters.AddWithValue("@CategoryId", categoryId);
                        insertProductCommand.Parameters.AddWithValue("@InventoryId", (object)inventoryId ?? DBNull.Value);
                        insertProductCommand.Parameters.AddWithValue("@Price", 9.99m + i);
                        insertProductCommand.Parameters.AddWithValue("@ImageUrl", $"https://via.placeholder.com/400?text=Product+{i}");
                        insertProductCommand.Parameters.AddWithValue("@ImageCompromise", "horizontal");
                        insertProductCommand.ExecuteNonQuery();
                    }

                    string insertPostSql = @"
                        INSERT INTO posts (title, content, imageurl, imagecompromise, dateposted)
                        SELECT @Title, @Content, @ImageUrl, @ImageCompromise, @DatePosted
                        WHERE NOT EXISTS (SELECT 1 FROM posts WHERE title = @Title)";

                    using (var insertPostCommand = new NpgsqlCommand(insertPostSql, connection))
                    {
                        insertPostCommand.Parameters.AddWithValue("@Title", $"Post {i}");
                        insertPostCommand.Parameters.AddWithValue("@Content", $"This is the content of post {i}.");
                        insertPostCommand.Parameters.AddWithValue("@ImageUrl", "https://via.placeholder.com/400x400");
                        insertPostCommand.Parameters.AddWithValue("@ImageCompromise", "vertical");
                        insertPostCommand.Parameters.AddWithValue("@DatePosted", DateTime.Now);
                        insertPostCommand.ExecuteNonQuery();
                    }

                }

                connection.Close();
            }
        }


        private async Task CreateUsersAndRoles()
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                var adminRole = new IdentityRole("Admin");
                await _roleManager.CreateAsync(adminRole);
            }

            if (!await _roleManager.RoleExistsAsync("User"))
            {
                var userRole = new IdentityRole("User");
                await _roleManager.CreateAsync(userRole);
            }

            if (await _userManager.FindByNameAsync("admin") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };

                adminUser.NormalizedUserName = adminUser.UserName.ToUpper();
                adminUser.NormalizedEmail = adminUser.Email.ToUpper();

                var result = await _userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("Admin user created and assigned to Admin role.");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error creating Admin user: {error.Description}");
                    }
                }
            }

            if (await _userManager.FindByNameAsync("user") == null)
            {
                var regularUser = new ApplicationUser
                {
                    UserName = "user@example.com",
                    Email = "user@example.com",
                    EmailConfirmed = true
                };

                regularUser.NormalizedUserName = regularUser.UserName.ToUpper();
                regularUser.NormalizedEmail = regularUser.Email.ToUpper();

                var result = await _userManager.CreateAsync(regularUser, "User@123");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(regularUser, "User");
                    Console.WriteLine("Regular user created and assigned to User role.");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error creating Regular user: {error.Description}");
                    }
                }
            }
        }
    }
}
