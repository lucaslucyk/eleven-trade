namespace CotpsBot.Models.Http
{
    public class UserInfo
    {
        public int is_can_use {get; set;}
        public int is_can_withdrawInternal {get; set;}
        public int is_sms_white {get; set;}
        public int is_withdraw_black {get; set;}
        public int is_withdraw_user {get; set;}
        public int is_ip_repeat {get; set;}
        public int is_need_face {get; set;}
        public int is_need_auth {get; set;}
        public int withdraw_step {get; set;}
        public string regType {get; set;}
        public int level {get; set;}
        public int root_uid {get; set;}
        public int share_uid {get; set;}
        public int parent_root_uid {get; set;}
        public int is_nomal {get; set;}
        public int is_id_auth {get; set;}
        public int inside {get; set;}
        public int deal_count {get; set;}
        public int deal_plan {get; set;}
        public int deal_num {get; set;}
        public long deal_time {get; set;}
        public double deal_profit {get; set;}
        public int fee {get; set;}
        public int in_fee {get; set;}
        public int del_address_count {get; set;}
        public int lixibao_profit {get; set;}
        public int recharge {get; set;}
        public int get_money {get; set;}
        public int inviteId {get; set;}
        public int invite2Id {get; set;}
        public int invite3Id {get; set;}
        public float total_one_profit {get; set;}
        public float one_profit {get; set;}
        public double has_one_profit {get; set;}
        public int total_two_profit {get; set;}
        public int two_profit {get; set;}
        public int has_two_profit {get; set;}
        public int total_three_profit {get; set;}
        public int three_profit {get; set;}
        public int has_three_profit {get; set;}
        public int total_four_profit {get; set;}
        public int four_profit {get; set;}
        public int has_four_profit {get; set;}
        public string avatar {get; set;}
        public string trans_password {get; set;}
        public string in_balance {get; set;}
        public string out_balance {get; set;}
        public string email {get; set;}
        public bool is_email_valid {get; set;}
        public bool is_mobile_valid {get; set;}
        public string mobile {get; set;}
        public bool locked {get; set;}
        public double balance {get; set;}
        public double frozen_balance {get; set;}
        public double freeze_balance {get; set;}
        public double lixibao_freeze_balance {get; set;}
        public long createTime {get; set;}
        public int status {get; set;}
        public int is_black {get; set;}
        public int is_steal_exchange {get; set;}
        public string username {get; set;}
        public int user_id {get; set;}
        public int id {get; set;}
        public string register_ip {get; set;}
        public string registe_time {get; set;}
        public string inviteCode {get; set;}
        public int __v {get; set;}
        public string ip { get; set; }
        public string accountTypeTxt {get; set;}
    }
}