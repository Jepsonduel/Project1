using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Threads_IPC_Project1
{
    class Customer : Bank
    {
        private string name;
        private string email;
        private int id;

        public Customer() { }
        public Customer(string name, string email, int id)
        {
            this.name = name;
            this.email = email;
            this.id = id;
        }
    }

    class Account : Bank
    {
        CreditCard card;
        private float balance;
        private string username;
        private string password;
        private int loginToken = 0;
        public static float transferStatus = 0;

        public Account() { }
        public Account(float balance, string username, string password, int creditScore)
        {
            this.balance = balance;
            this.username = username;
            this.password = password;
            card = new CreditCard(creditScore);
        }

        public void deposit(float amount)
        {
            if (loginToken == 1 && amount > 0)
            {
                Console.WriteLine("Depositing ${0} for {1}", amount, username);
                balance += amount;
                Console.WriteLine("${0} has been deposited for {1}", amount, username);
            }
            else if (loginToken == 1 && amount <= 0)
            {
                Console.WriteLine("Invalid transaction amount");
            }
            else
            {
                Console.WriteLine("Must log in");
            }
        }

        public float withdraw(float amount)
        {
            if (loginToken == 1)
            {
                if (balance <= 0 || amount > balance)
                {
                    Console.WriteLine("Insufficient funds");
                    return 0;
                }
                Console.WriteLine("Processing withdrawal of ${0}", amount);
                balance -= amount;
                Console.WriteLine("${0} has been withdrawn for {1}", amount, username);
                return amount + balance;
            }
            Console.WriteLine("Must log in");
            return 0;
        }

        public void login(string username, string password)
        {
            if (loginToken == 1)
            {
                throw new Exception("Duplicate account access found: This user is already logged in");
            }
            else if (username == null || password == null)
            {
                Console.WriteLine("Must enter username or password");
            }
            else if (username != this.username && password != this.password)
            {
                Console.WriteLine("Incorrect username or passowrd");
            }
            else if (username.Equals(this.username) && password == this.password)
            {
                Console.WriteLine("User {0} has been logged in", username);
                loginToken = 1;
            }
        }

        public void logout()
        {
            if (loginToken == 0)
            {
                throw new Exception("Duplicate account access found: This user is already logged out");
            }
            else
            {
                Console.WriteLine("User: {0} has logged out", username);
                loginToken = 0;
            }

        }

        public float loan(float amount)
        {
            if (loginToken == 1)
            {
                if (card.getCreditScore() >= 500)
                {
                    Console.WriteLine("${0} has been disbursed for {1}", amount, username);
                    return amount;
                }
                Console.WriteLine("User: {0} is not eligible for a loan", username);
                return 0;
            }
            Console.WriteLine("Must log in");
            return 0;
        }

        public float transfer(float amount, Account account1, Account account2)
        {
            if (loginToken == 1)
            {
                if (amount <= 0)
                {
                    Console.WriteLine("Invalid transfer amount");
                    return 0;
                }
                else if (amount > balance)
                {
                    Console.WriteLine("Insufficient funds");
                    return 0;
                }
                else
                {
                    Console.WriteLine("Transfer processing for {0} from {1} to {2}", amount, account1, account2);
                    Console.WriteLine("Transfer processed for {0} from {1} to {2}", amount, account1, account2);
                    balance -= amount;
                    account2.recieve(amount);
                    return amount;
                }
            }
            Console.WriteLine("Must log in");
            return 0;
        }

        public void recieve(float amount)
        {
            balance += amount;
        }

        public string getUsername() { return username; }
        public string getPassword() { return password; }
        public float getBalance() { return balance; }
        public float getTransferStatus() { return transferStatus; }
    }

    class CreditCard : Account
    {
        private int cardNumber;
        private int creditScore;

        public CreditCard() { }
        public CreditCard(int creditScore)
        {
            this.creditScore = creditScore;
        }
        public CreditCard(int cardNumber, int creditScore)
        {
            this.cardNumber = cardNumber;
            this.creditScore = creditScore;
        }

        public int getCardNumber() { return cardNumber; }
        public int getCreditScore() { return creditScore; }
    }

    class Bank
    {
        private int id = 1000;
        private string name;

        public Bank() { }

        public Account signUp(string name, string email)
        {
            Customer customer = new Customer(name, email, id + 1);

            Console.WriteLine("Please enter username: ");
            string user = Convert.ToString(Console.ReadLine());
            Console.WriteLine("Please enter password: ");
            string pass = Convert.ToString(Console.ReadLine());
            Account account = new Account(0, user, pass, 500);

            Console.WriteLine("\nAccount has been created");
            return account;
        }

    }

    class Program
    {
        private static object locked = new object();
        private static Mutex mutex = new Mutex(), x = new Mutex(), y = new Mutex();

        public static void transactionWithdraw(Account account, float amount)
        {
            mutex.WaitOne();
            account.login(account.getUsername(), account.getPassword());
            account.withdraw(amount);
            account.logout();
            mutex.ReleaseMutex();
        }

        public static void transactionDeposit(Account account, float amount)
        {
            mutex.WaitOne();
            account.login(account.getUsername(), account.getPassword());
            account.deposit(amount);
            account.logout();
            mutex.ReleaseMutex();
        }

        public static void transactionLoan(Account account, float amount)
        {
            mutex.WaitOne();
            account.login(account.getUsername(), account.getPassword());
            account.loan(amount);
            account.logout();
            mutex.ReleaseMutex();
        }

        public static void accountStuff(Account account, float deposit, float loan, float withdraw)
        {
            try
            {
                Monitor.Enter(locked);
                account.login(account.getUsername(), account.getPassword());
                account.deposit(deposit);
                account.loan(loan);
                account.withdraw(withdraw);
                account.logout();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                Monitor.Exit(locked);
            }
        }

        public static void transfer1(float amount, Account account1, Account account2)
        {
            try
            {
                if (x.WaitOne())
                {
                    y.ReleaseMutex();
                }
                else if (y.WaitOne())
                {
                    x.ReleaseMutex();
                }
                x.WaitOne();
                y.WaitOne();
                account1.login(account1.getUsername(), account1.getPassword());
                account1.transfer(amount, account1, account2);
                account1.logout();
            }
            catch (Exception e)
            {
                Console.WriteLine("Deadlock reached");
            }
            finally
            {
                
                x.ReleaseMutex();
                y.ReleaseMutex();
            }
        }

        public static void transfer2(float amount, Account account2, Account account1)
        {
            try
            {
                if (y.WaitOne())
                {
                    x.ReleaseMutex();
                }
                else if (x.WaitOne())
                {
                    y.ReleaseMutex();
                }
                y.WaitOne();
                x.WaitOne();
                account2.login(account2.getUsername(), account2.getPassword());
                account2.transfer(amount, account2, account1);
                account2.logout();
            }
            catch (Exception e)
            {
                Console.WriteLine("Deadlock reached");
            }
            finally
            {
                
                x.ReleaseMutex();
                y.ReleaseMutex();
            }

        }


        public static void Main(string[] args)
        {
            List<Thread> threads = new List<Thread>();
            Thread mainThread = Thread.CurrentThread;
            Account account1 = new Account(150.00f, "JohnKennedy", "89FLY", 850);
            Account account2 = new Account(289.27f, "RachelRyan", "405Cover1", 850);
            Account account3 = new Account(1289.45f, "StevenMare", "Softway21", 450);

            for (int i = 0; i < 10; i++)
            {
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    transactionDeposit(account1, 250.00f);
                    transactionLoan(account1, 500.00f);
                    transactionWithdraw(account1, 150.00f);
                }));
                threads.Add(thread);
                thread.Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            Console.WriteLine("\nTest 1 Complete\n");
            threads = new List<Thread>();

            for (int i = 0; i < 10; i++)
            {
                Thread thread = new Thread(new ThreadStart(() => accountStuff(account3, 679.28f, 2000.00f, 250.99f)));
                threads.Add(thread);
                thread.Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            Console.WriteLine("\nTest 2 Complete\n");
            threads = new List<Thread>();

            for (int i = 0; i < 3; i++)
            {
                Thread thread1 = new Thread(new ThreadStart(() => transfer1(200.00f, account1, account2)));
                Thread thread2 = new Thread(new ThreadStart(() => transfer2(200.00f, account2, account1)));
                threads.Add(thread1);
                threads.Add(thread2);
                thread1.Start();
                thread2.Start();

            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            Console.WriteLine("\nTest 3 Complete\n");

            Console.ReadKey();
        }
    }
}

