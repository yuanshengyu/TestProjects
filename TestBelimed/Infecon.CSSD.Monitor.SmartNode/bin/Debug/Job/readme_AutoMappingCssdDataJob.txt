AutoMappingCssdDataJob
作用：自动匹配传感器记录和CSSD记录
参数：
	TimeDifference		时间差（必须）

	<!--定义 自动匹配 任务-->
    <job>
      <name>AutoMappingCssdDataJob1</name>
      <group>AutoMappingCssdDataJobGroup1</group>
      <description>自动匹配传感器记录和CSSD记录</description>
      <job-type>Infecon.CSSD.Monitor.Common.Job.AutoMappingCssdDataJob,Infecon.CSSD.Monitor.Common</job-type>

      <durable>true</durable>
      <recover>false</recover>
      <job-data-map>
        <!--参数：TimeDifference	时间差（必须）-->
        <entry>
          <key>TimeDifference</key>
          <value>10</value>
        </entry>
      </job-data-map>
    </job>

    <!--定义 自动匹配 任务 触发器-->
    <trigger>
      <cron>
        <name>AutoMappingCssdDataJobTrigger1</name>
        <group>AutoMappingCssdDataJobTriggerGroup1</group>
        <job-name>AutoMappingCssdDataJob1</job-name>
        <job-group>AutoMappingCssdDataJobGroup1</job-group>
        <!--每30分钟执行一次任务-->
        <cron-expression>0 0/30 * * * ?</cron-expression>
      </cron>
    </trigger>