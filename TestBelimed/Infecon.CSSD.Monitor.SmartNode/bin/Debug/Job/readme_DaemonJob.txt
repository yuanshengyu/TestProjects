DaemonJob
作用：定时检测程序是否已经运行，如果没有运行则启动程序，然后等待。
参数：
	AppPath		程序全路径（必须）
	AppParams	启动参数（可选）

	<!--定义守护进程任务 Job-->
    <job>
      <name>DaemonJob1</name>
      <group>DaemonJob1Group</group>
      <description>Quartz.Net示例任务</description>
      <job-type>Infecon.CSSD.Monitor.Common.Job.DaemonJob,Infecon.CSSD.Monitor.Common</job-type>

      <durable>true</durable>
      <recover>false</recover>
      <job-data-map>
        <!--参数：AppPath 程序全路径（必须）-->
        <entry>
          <key>AppPath</key>
          <value>E:\Program Files\Foxit Software\Foxit Reader\Foxit Reader.exe</value>
        </entry>
        <!--参数：AppParams 启动参数（可选）-->
        <entry>
          <key>AppParams</key>
          <value>"C:\Users\Jun\Downloads\IoC容器和DependencyInjection模式.pdf"</value>
        </entry>
      </job-data-map>
    </job>

    <!--定义守护进程任务 触发器 每30秒执行一次DemoJob任务-->
    <trigger>
      <cron>
        <name>DaemonJobTrigger</name>
        <group>DaemonJobTriggerGroup</group>
        <job-name>DaemonJob1</job-name>
        <job-group>DaemonJob1Group</job-group>
        <cron-expression>0/30 * * * * ?</cron-expression>
      </cron>
    </trigger>