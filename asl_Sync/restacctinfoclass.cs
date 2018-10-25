using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace asl_SyncLibrary
{
    public class RestInfo
    {
        // PRIVATE INTEGERS
        private int nperskassa;
        private int npersnr;
        private int nkassanr;

        // PRIVATE STRINGS
        private string nkartennr;
        private string bcode;
        private string sbcode;
        private string skey;
        private string firstname;
        private string lastname;
        private string passstatus;
        private string invoice;
        private string disctype;
        private string chrgflag;
        private string tokenkey;
        private string dob;
        private string persdesc;

        // PUBLIC INTEGERS
        public int Nperskassa
        {
            get { return nperskassa; }
            set { nperskassa = value; }
        }
        public int Npersnr
        {
            get { return npersnr; }
            set { npersnr = value; }
        }
        public int Nkassanr
        {
            get { return nkassanr; }
            set { nkassanr = value; }
        }

        // PUBLIC STRINGS
        public string Persdesc
        {
            get { return persdesc; }
            set { persdesc = value; }
        }
        public string Dob
        {
            get { return dob; }
            set { dob = value; }
        }
        public string Tokenkey
        {
            get { return tokenkey; }
            set { tokenkey = value; }
        }
        public string Chrgflag
        {
            get { return chrgflag; }
            set { chrgflag = value; }
        }
        public string Disctype
        {
            get { return disctype; }
            set { disctype = value; }
        }
        public string Nkartennr
        {
            get { return nkartennr; }
            set { nkartennr = value; }
        }
        public string Bcode
        {
            get { return bcode; }
            set { bcode = value; }
        }
        public string Sbcode
        {
            get { return sbcode; }
            set { sbcode = value; }
        }
        public string Skey
        {
            get { return skey; }
            set { skey = value; }
        }
        public string Firstname
        {
            get { return firstname; }
            set { firstname = value; }
        }
        public string Lastname
        {
            get { return lastname; }
            set { lastname = value; }
        }
        public string Passstatus
        {
            get { return passstatus; }
            set { passstatus = value; }
        }
        public string Invoice
        {
            get { return invoice; }
            set { invoice = value; }
        }
    }
}
