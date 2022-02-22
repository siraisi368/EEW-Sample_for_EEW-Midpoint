﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;

namespace EEW_Sample_for_EEW_Midpoint
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private readonly HttpClient client = new HttpClient();

        private async void eew_timer_Tick(object sender, EventArgs e)
        {
            try
            {
                DateTime dt = DateTime.Now; //現在時刻の取得(PC時刻より)
                var tm = dt.AddSeconds(-2); //現在時刻から2秒引く(取得失敗を防ぐため)
                var time = tm.ToString("yyyyMMddHHmmss");//時刻形式の指定(西暦/月/日/時/分/秒)

                var url = $"http://127.0.0.1:58722/kyomoni.json"; //EEW-Midpoint

                var json = await client.GetStringAsync(url);                 //awaitを用いた非同期JSON取得
                var eew = JsonConvert.DeserializeObject<EEW>(json);          //EEWクラスを用いてJSONを解析(デシリアライズ)

                //JSONの中から使うデータを指定して使いやすいように名前を変えます
                var repo_time = eew.report_time;                             // 取得時刻(string)
                var reg_name = eew.region_name;                              // 震源地名(string)
                var latitude = eew.latitude;                                 // 緯度(string(本来はfloat))
                var longtude = eew.longitude;                                // 経度(string(本来はfloat))
                var depth = eew.depth;                                       // 深さ(string)
                var max_int = eew.calcintensity;                             // 予測震度(string)
                var mag = eew.magunitude;                                    // マグニチュード(string(本来はfloat))
                bool end_flg = eew.is_final == "true";                       // 最終報フラグ(bool)
                var repo_num = eew.report_num;                               // 報番(string(本来はint))
                var ori_time = eew.origin_time;                              // 発生時刻(string)
                var aler_flg = eew.alertflg;                                 // 警報フラグ(string)
                var eew_flg = eew.result.message;                            // EEWフラグ(string)
                string eew_flgs = null;

                //種別判別(これをAPIレベルでやれるようになってほしい)
                if (eew_flg != "データがありません")        //eew_flg が true のとき
                {
                    if (aler_flg == "予報")                 //aler_flg が 予報 のとき
                    {
                        eew_flgs = "fore";                  //eew_flgs に "fore" を代入

                        if (end_flg == true)                //aler_flg が 予報 で end_flg が true のとき
                        {
                            eew_flgs = "fore_end";          //eew_flgs に "fore_end" を代入
                        }
                    }

                    if (aler_flg == "警報")                 //aler_flg が 警報 のとき
                    {
                        eew_flgs = "war";                   //eew_flgs に "war" を代入

                        if (end_flg == true)                //aler_flg が 警報 で end_flg が true のとき
                        {
                            eew_flgs = "war_end";           //eew_flgs に "war_end" を代入
                        }
                    }
                }
                else                                        //eew_flg が false のとき
                {
                    eew_flgs = "none";                      //eew_flgs に "none" を代入
                }

                //実処理部分(switch文を使ってけ)
                switch (eew_flgs)
                {
                    case "fore":
                        label1.Text = $"緊急地震速報(予報)  第{repo_num}報  {reg_name}で地震  最大震度{max_int}\r\nマグニチュード{mag}  震源の深さ:{depth}";
                        break;
                    case "fore_end":
                        label1.Text = $"緊急地震速報(予報)  最終報  {reg_name}で地震  最大震度{max_int}\r\nマグニチュード{mag}  震源の深さ:{depth}";
                        break;
                    case "war":
                        label1.Text = $"緊急地震速報(警報)  第{repo_num}報  {reg_name}で地震  最大震度{max_int}\r\nマグニチュード{mag}  震源の深さ:{depth}";
                        break;
                    case "war_end":
                        label1.Text = $"緊急地震速報(警報)  最終報  {reg_name}で地震  最大震度{max_int}\r\nマグニチュード{mag}  震源の深さ:{depth}";
                        break;
                    case "none":
                        label1.Text = "EEWが発表されていません";
                        break;
                }
            }
            catch (Exception ex)
            {

                eew_timer.Enabled = false;
                await Task.Delay(100);
                eew_timer.Enabled = true;
            }
        }
    }
}
