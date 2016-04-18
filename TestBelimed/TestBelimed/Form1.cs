using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Quartz;
using Quartz.Impl;

namespace TestBelimed
{
    public partial class Form1 : Form
    {
        IScheduler? fa;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ISchedulerFactory factory = new StdSchedulerFactory();
            IScheduler fa = factory.GetScheduler();
            IJobDetail job = JobBuilder.Create<TestQ>().WithIdentity("job1", "group1").Build();
            //ITrigger trigger = TriggerBuilder.Create().WithIdentity("trigger1", "group1").StartAt(DateBuilder.FutureDate(2, IntervalUnit.Hour)).WithSimpleSchedule(x => x.RepeatHourlyForever()).ModifiedByCalendar("holidays").Build();
            //ITrigger triggerRead = TriggerBuilder.Create()
            //           .WithIdentity(mName + "_" + cTriggerIdSensorDataRead + "_" + s.SensorID, cPluginName)
            //           .StartNow()
            //           .WithSimpleSchedule(x => x.WithInterval(ReadingInterval).RepeatForever())
            //           .Build();

            ITrigger tigger = TriggerBuilder.Create().WithIdentity("job1", "group1").StartNow().Build();
            fa.ScheduleJob(job, tigger);
            //DateTime runTime = TriggerUtils.ComputeFireTimes(
            //Quartz.MisfireInstruction.SimpleTrigger trigger = new Quartz.MisfireInstruction.SimpleTrigger();
            job = JobBuilder.Create<TestQ2>().WithIdentity("job2", "guoup1").Build();
            tigger = TriggerBuilder.Create().WithIdentity("job2", "group1").StartNow().Build();
            fa.ScheduleJob(job, tigger);
            //job.Durable = (true);
        }

        private void button1_Click(object sender, EventArgs e)//Start
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }
    }
}
