namespace PPTWebApp.Tests.Fixtures;

using Npgsql;
using System;
using Testcontainers.PostgreSql;

public static class SharedPostgresContainer
{
    public static PostgreSqlContainer PostgresContainer { get; }

    static SharedPostgresContainer()
    {
        PostgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .Build();

        PostgresContainer.StartAsync().Wait();

        InitializeDatabase();
    }

    public static string GetConnectionString()
    {
        return PostgresContainer.GetConnectionString();
    }

    public static void StopContainer()
    {
        PostgresContainer.DisposeAsync().AsTask().Wait();
    }


    private static void InitializeDatabase()
    {
        CreateTables();
        CreateTriggers();

    }


    private static void CreateTables()
    {
        using (var connection = new NpgsqlConnection(GetConnectionString()))
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
                        userid UUID PRIMARY KEY,
                        firstname VARCHAR(255),
                        lastname VARCHAR(255),
                        telephone VARCHAR(20),
                        createdat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        modifiedat TIMESTAMP,
                        CONSTRAINT fk_userprofiles_aspnetusers FOREIGN KEY (userid) REFERENCES aspnetusers(id) ON DELETE CASCADE
                    );

                    CREATE TABLE IF NOT EXISTS useractivity (
                        userid UUID PRIMARY KEY,
                        lastactivityat TIMESTAMP NOT NULL,
                        CONSTRAINT fk_useractivity_aspnetusers FOREIGN KEY (userid) REFERENCES aspnetusers(id) ON DELETE CASCADE
                    );

                    CREATE TABLE IF NOT EXISTS useraddresses (
                        id SERIAL PRIMARY KEY,
                        userid UUID NOT NULL,
                        addressline1 VARCHAR(255) NOT NULL,
                        addressline2 VARCHAR(255),
                        city VARCHAR(100),
                        postalcode VARCHAR(20),
                        county VARCHAR(100),
                        telephone VARCHAR(20),
                        mobile VARCHAR(20),
                        CONSTRAINT fk_useraddresses_aspnetusers FOREIGN KEY (userid) REFERENCES aspnetusers (id) ON DELETE CASCADE
                    );

                    CREATE TABLE IF NOT EXISTS userpayments (
                        id SERIAL PRIMARY KEY,
                        userid UUID NOT NULL,
                        paymenttype VARCHAR(50),
                        provider VARCHAR(255),
                        accountno VARCHAR(50),
                        expiry DATE,
                        CONSTRAINT fk_userpayments_aspnetusers FOREIGN KEY (userid) REFERENCES aspnetusers (id) ON DELETE CASCADE
                    );

                    CREATE TABLE IF NOT EXISTS orderdetails (
                        id SERIAL PRIMARY KEY,
                        userid UUID NOT NULL,
                        total DECIMAL(10, 2) NOT NULL,
                        paymentid INT REFERENCES userpayments(id) ON DELETE SET NULL,
                        createdat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        modifiedat TIMESTAMP,
                        CONSTRAINT fk_orderdetails_aspnetusers FOREIGN KEY (userid) REFERENCES aspnetusers (id) ON DELETE SET NULL
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

    private static void CreateTriggers()
    {
        using (var connection = new NpgsqlConnection(GetConnectionString()))
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
}
