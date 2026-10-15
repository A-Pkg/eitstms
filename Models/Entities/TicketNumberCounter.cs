namespace EITSTMS.Models.Entities
{
    public class TicketNumberCounter
    {
        public DateTime CounterDate { get; set; } // 流水号所属日期

        public int CurrentNumber { get; set; } // 当天已经使用到的流水号
    }
}