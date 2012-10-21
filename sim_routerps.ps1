Function addToConfigTable ($config_info, $sim, $node)
{
	
	$conn = New-Object "System.Data.SqlClient.SqlConnection"
	$conn.ConnectionString = "server=" + $node + ";database=" + $DBname + ";Trusted_Connection=true;Asynchronous Processing = true"
	write-host $conn.ConnectionString
	$conn.Open()
	
	
	# $query = "INSERT " + $config_info[3] + "config VALUES("
	$query = "INSERT SimulationDB.testing.config VALUES("
	$i = 0
	$last = $config_info.length - 1
	foreach($item in $config_info)
	{
		$query = $query + $item
		if($i -ne $last)
		{
			 $query = $query + ", "
		}
		$i = $i + 1
	}
	$query = $query + ")"
	write-host $query
	$cmd = new-object "System.Data.SqlClient.SqlCommand" ($query, $conn)
	$cmd.ExecuteNonQuery() | out-null
	$conn.close()
}




$root_dir = "C:\Users\crankshaw\Documents\router_testing\"
$DBname = "SimulationDB"

$simulations = get-childitem -path $root_dir
$servers = @("\\gw18\Indra\", "\\gw18\Indra\")
$nodes = @("gw18", "gw18")
$snap_done_flag = "\done.txt"


$i = 0
$mybool = 0
foreach ($sim in $simulations)
{
	#write-host $sim.name
	$snaps = get-childitem -path $sim.FullName
	$j = 0
	foreach($snapshot in $snaps)
	{
		$done = $snapshot.FullName + $snap_done_flag
		
		#write-host (test-path $done)
		if(test-path $done)
		{
			$dest_serv = $i % $servers.length
			$current_node = ($nodes[$dest_serv])
			# $dest_path = $servers[$dest_serv] + "unprocessed\" + $sim.name + "\" + $snapshot.name
			$dest_path = "'" + $servers[$dest_serv] + "test\" + $sim.name + "\" + $snapshot.name + "'"
			#robocopy $snapshot.FullName $dest_path /Z /MIR /S
			$post_process = "'" + $servers[$dest_serv] + "\processed\" + $sim.name + "\" + $snapshot.name + "'"
			$simtable_pref = "'" + $DBname + "." + $sim.name + ".'"
			$config_info = @($i, $dest_path, $post_process, $simtable_pref)
			addToConfigTable $config_info $sim $current_node
		}
		$j = $j + 1
	}
	$i = $i + 1
}

