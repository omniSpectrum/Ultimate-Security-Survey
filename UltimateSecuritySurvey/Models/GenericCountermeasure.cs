//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UltimateSecuritySurvey.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class GenericCountermeasure
    {
        public GenericCountermeasure()
        {
            this.CustomerAnswers = new HashSet<CustomerAnswer>();
            this.CustomerAnswers1 = new HashSet<CustomerAnswer>();
            this.CustomerAnswers2 = new HashSet<CustomerAnswer>();
            this.GenericCountermeasure1 = new HashSet<GenericCountermeasure>();
        }
    
        public int countermeasureId { get; set; }
        public int questionId { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public System.DateTime dateAndTime { get; set; }
        public Nullable<int> motherCountermeasure { get; set; }
    
        public virtual ICollection<CustomerAnswer> CustomerAnswers { get; set; }
        public virtual ICollection<CustomerAnswer> CustomerAnswers1 { get; set; }
        public virtual ICollection<CustomerAnswer> CustomerAnswers2 { get; set; }
        public virtual ICollection<GenericCountermeasure> GenericCountermeasure1 { get; set; }
        public virtual GenericCountermeasure GenericCountermeasure2 { get; set; }
        public virtual Question Question { get; set; }
    }
}