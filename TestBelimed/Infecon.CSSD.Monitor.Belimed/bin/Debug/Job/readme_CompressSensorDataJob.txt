CompressSensorDataJob
作用：压缩已完成的监控数据
参数：无

	<job>
      <name>CompressSensorDataJob1</name>
      <group>CompressSensorDataJob1Group</group>
      <description>压缩监控数据</description>
      <job-type>Infecon.CSSD.Monitor.Common.Job.CompressSensorDataJob,Infecon.CSSD.Monitor.Common</job-type>

      <durable>true</durable>
      <recover>false</recover>
    </job>
    <trigger>
      <cron>
        <name>CompressSensorDataTrigger1</name>
        <group>CompressSensorDataTrigger1Group</group>
        <job-name>CompressSensorDataJob1</job-name>
        <job-group>CompressSensorDataJob1Group</job-group>
        <cron-expression>0 0/5 * * * ?</cron-expression>
      </cron>
    </trigger>