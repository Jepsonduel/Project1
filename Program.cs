using System;
using System.Collections.Generic;
using System.Threading;

namespace Threads_IPC_Project1
{

    class Account
    {
        CreditCard card;
        private float balance;
        private string username;
        private string password;
        private int loginToken = 0; // value reads 0 for logged out and 1 for logged in

        public Account() { } //default constructor
        public Account(float balance, string username, string password, int creditScore)
        {
            this.balance = balance;
            this.username = username;
            this.password = password;
            card = new CreditCard(creditScore);
        }

        /*
         This method adds a specified amount into the account balance.
         */
        public void deposit(float amount)
        {
            if (loginToken == 1 && amount > 0) // verifies user is logged in and amount is not negative or 0
            {
                Console.WriteLine("Depositing ${0} for {1}", amount, username);
                balance += amount;
                Console.WriteLine("${0} has been deposited for {1}", amount, username);
            }
            else if (loginToken == 1 && amount <= 0) // invalid case
            {
                Console.WriteLine("Invalid transaction amount");
            }
            else
            {
                Console.WriteLine("Must log in"); // invalid token
            }
        }

        /*
         This method takes a specified amount from the account balance and returns the value.
         */
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
                balance -= amount; // amount deducted
                Console.WriteLine("${0} has been withdrawn for {1}", amount, username);
                return amount;
            }
            Console.WriteLine("Must log in");
            return 0;
        }

        /*
         This method validates the account token to value 1, granting access to other methods.
         */
        public void login(string username, string password)
        {
            if (loginToken == 1) // token already valid
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
                Console.WriteLine("User {0} has been logged in", username); // token validated, user logs in
                loginToken = 1;
            }
        }

        /*
         This method sets the account token value to 0, locking access to other methods.
         */
        public void logout() 
        {
            if (loginToken == 0) // token already 0
            {
                throw new Exception("Duplicate account access found: This user is already logged out");
            }
            else
            {
                Console.WriteLine("User: {0} has logged out", username); // user logs out
                loginToken = 0; // token set to 0
            }

        }

        /*
        This method takes a specified amount requested and adds it to the account balance. The value is then returned. 
         */
        public float loan(float amount)
        {
            if (loginToken == 1)
            {
                if (card.getCreditScore() >= 500) // credit score value checked
                {
                    Console.WriteLine("${0} has been disbursed for {1}", amount, username);
                    balance += amount;
                    return amount;
                }
                Console.WriteLine("User: {0} is not eligible for a loan", username); // invalid credit score
                return 0;
            }
            Console.WriteLine("Must log in");
            return 0;
        }

        /*
         This method takes in two accounts and an amount, deducts the specified amount from the user's balance and adds deducted amount to specified account.
        */
        public float transfer(float amount, Account account1, Account account2)
        {
            if (loginToken == 1)
            {
                if (amount <= 0) // amount cannot be negative or 0
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
                    balance -= amount; // amount deducted from account1
                    account2.recieve(amount); // amount added to account2
                    return amount;
                }
            }
            Console.WriteLine("Must log in");
            return 0;
        }

        /*
         This method takes a specified amount and adds it to the account balance.
         */
        public void recieve(float amount)
        {
            balance += amount;
        }

        /*
         Retrieval methods for username, password, and balance.
         */
        public string getUsername() { return username; }
        public string getPassword() { return password; }
        public float getBalance() { return balance; }
    }

    class CreditCard : Account
    {
        private int cardNumber; // card id
        private int creditScore; // this score determines the owner's eligibility for a loan.

        public CreditCard() { } // default constructor
        public CreditCard(int creditScore)
        {
            this.creditScore = creditScore;
        }
        public CreditCard(int cardNumber, int creditScore)
        {
            this.cardNumber = cardNumber;
            this.creditScore = creditScore;
        }

        /*
         Retreival methods for cardNumber and creditScore
         */
        public int getCardNumber() { return cardNumber; }
        public int getCreditScore() { return creditScore; }
    }

    class Program
    {
        private static object locked = new object(); // locker object
        private static Mutex mutex = new Mutex(), x = new Mutex(), y = new Mutex(); // mutex locks

        /*
         This method takes in an account and a specified amount. It logs into the specified account, calls the withdraw method on the account, and logs out of the account.
        Mutex locks prevent race conditions.
         */
        public static void transactionWithdraw(Account account, float amount)
        {
            mutex.WaitOne();
            account.login(account.getUsername(), account.getPassword());
            account.withdraw(amount);
            account.logout();
            mutex.ReleaseMutex();
        }

        /*
         This method takes in an account and a specified amount. It logs into the specified account, calls the deposit method on the account, and logs out of the account.
        Mutex locks prevent race conditions.
         */
        public static void transactionDeposit(Account account, float amount)
        {
            mutex.WaitOne();
            account.login(account.getUsername(), account.getPassword());
            account.deposit(amount);
            account.logout();
            mutex.ReleaseMutex();
        }

        /*
         This method takes in an account and a specified amount. It logs into the specified account, calls the withdraw loan on the account, and logs out of the account.
        Mutex locks prevent race conditions.
         */
        public static void transactionLoan(Account account, float amount)
        {
            mutex.WaitOne();
            account.login(account.getUsername(), account.getPassword());
            account.loan(amount);
            account.logout();
            mutex.ReleaseMutex();
        }

        /*
         This method takes in an account, deposit, loan, and withdrawal amount. It logs into the account, calls related methods, and logs out.
        Monitor locks prevent race conditions.
         */
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

        /*
         This method is meant to demonstrate deadlock by aquiring two mutex locks, initiating an account transfer, and releasing the locks within a try catch statement.
         */
        public static void transfer1(float amount, Account account1, Account account2)
        {
            try
            {
                x.WaitOne();
                y.WaitOne();
                account1.login(account1.getUsername(), account1.getPassword());
                account1.transfer(amount, account1, account2);
                account1.logout();
            }
            catch (Exception e) // intended for deadlock resolution
            {
                Console.WriteLine("Deadlock reached");
            }
            finally
            {
                
                x.ReleaseMutex();
                y.ReleaseMutex();
            }
        }

        /*
         This method is meant to demonstrate deadlock by aquiring two mutex locks, initiating an account transfer, and releasing the locks within a try catch statement.
         */
        public static void transfer2(float amount, Account account2, Account account1)
        {
            try
            {
                y.WaitOne();
                x.WaitOne();
                account2.login(account2.getUsername(), account2.getPassword());
                account2.transfer(amount, account2, account1);
                account2.logout();
            }
            catch (Exception e)
            {
                Console.WriteLine("Deadlock reached"); // intended for deadlock resolution
            }
            finally
            {
                
                x.ReleaseMutex();
                y.ReleaseMutex();
            }

        }


        public static void Main(string[] args)
        {
            List<Thread> threads = new List<Thread>(); // thread list for thread ordering
            Thread mainThread = Thread.CurrentThread;
            Account account1 = new Account(150.00f, "JohnKennedy", "89FLY", 850); // custom accounts initialized
            Account account2 = new Account(289.27f, "RachelRyan", "405Cover1", 850);
            Account account3 = new Account(1289.45f, "StevenMare", "Softway21", 450);

            for (int i = 0; i < 10; i++) // creates 10 concurrent threads running processes for deposit, loan, and withdrawal
            {
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    transactionDeposit(account1, 250.00f);
                    transactionLoan(account1, 500.00f);
                    transactionWithdraw(account1, 150.00f);
                })); // thread initialized
                threads.Add(thread); // added to list
                thread.Start(); // thread started
            }

            foreach (Thread thread in threads)
            {
                thread.Join(); // join method for ordering
            }

            Console.WriteLine("\nTest 1 Complete\n");
            threads = new List<Thread>();

            for (int i = 0; i < 10; i++) // creates 10 concurrent threads calling accountStuff process
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

            for (int i = 0; i < 3; i++) // 6 threads inteneded to demonstrate deadlock
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

