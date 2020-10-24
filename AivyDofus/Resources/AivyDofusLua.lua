-- variable global : 
-- 	- multi_proxy : DofusMultiProxy
-- 	- protocol_manager : BotofuProtocolManager
--  - protocol_dofus2 : BotofuProtocol 
--	- proxy_handlers : LuaHandler
-- 	- sleeper : CodeSleep
--  TODO

-- proxy functions ---
--
-- int -> ProxyEntity 																							-- default start from port return ProxyEntity
function default(port)
	return multi_proxy:Active(ProxyCallbackTypeEnum.Dofus2, true, port, 'D:\\AppDofus', 'Dofus')
end
-- ProxyData * int -> ProxyEntity 																				-- start from ProxyData and port return ProxyEntity
function start_proxy_from_config(config, port)
	--if config.HookRedirectionIp ~= '127.0.0.1' then do return start_remote_proxy_from_config(config, port) end
	return multi_proxy:Active(config.Type, true, port, config.FolderPath, config.ExeName)
end

function start_remote_proxy_from_config(config, port)
	return multi_proxy:RemoteActive(config.Type, true, port, config.HookRedirectionIp, config.FolderPath, config.ExeName)
end
-- string -> ProxyData 																							-- start from string return ProxyData 
-- read ProxyData from ./proxy_api_information.json  
function get_config(name)
	return multi_proxy._proxy_api:GetData(function(data) return data.Name == name end)
end

-- () -> () 
-- update protocol_dofus2 
function update_dofus2_protocol()
	protocol_dofus2 = protocol_manager[ProxyCallbackTypeEnum.Dofus2]
end

-- string -> NetworkElement
function get_message(name)
	return protocol_dofus2:Get(ProtocolKeyEnum.Messages, function(el) return el.name == name end)
end
-- string -> NetworkElement
function get_type(name)
	return protocol_dofus2:Get(ProtocolKeyEnum.Types, function(el) return el.name == name end)	
end

-- () -> ClientEntity
-- get remote ClientEntity from accept_callback where ClientEntity.IsGameClient 
function get_main_remote()
	return accept_callback:_main_remote_client()
end
-- () -> ClientEntity
-- get local ClientEntity from accept_callback where ClientEntity.IsGameClient
function get_main_local()
	return accept_callback:_main_local_client()
end

-- ProxyEntity * ClientEntity * string -> bool
-- send string (ASCII) message 
function send_string_message(local_proxy, client, message)
	if local_proxy == nil then return false
	elseif client == nil then return false
	elseif message == nil then return false
	else		
		multi_proxy[local_proxy.Port]._client_sender:Handle(client, message)
		return true
	end
end

-- ProxyEntity * bool * ClientEntity * NetworkElement * NetworkContentElement * uint -> bool
-- send byte[] message
-- parse content as byte[] with MessageDataBufferWriter
-- then build header as byte[] and add content as byte[] with MessageBufferWriter
function send_message(local_proxy, from_client, client, message, message_content, instance_id)
	if local_proxy == nil then return false
	elseif client == nil then return false
	elseif message == nil then return false
	elseif message_content == nil then return false
	else
		local data = MessageDataBufferWriter(message):Parse(message_content)		
		local final_data_writer = MessageBufferWriter(from_client):Build(message.protocolID, instance_id, data)
		local final_data = final_data_writer.Data
		final_data_writer:Dispose()
		multi_proxy[local_proxy.Port]._client_sender:Handle(client, final_data)
		return true
	end
end

-- game actions
--
-- ProxyEntity * ClientEntity * byte * string -> ()
-- send chat message
-- increase GLOBAL_INSTANCE_ID if success
function send_chat(proxy, client, channel, content)
	local instance_id = proxy.GLOBAL_INSTANCE_ID + 1
	local message = get_message('ChatClientMultiMessage') --protocol_dofus2:Get(ProtocolKeyEnum.Messages, function(el) return el.name == 'ChatClientMultiMessage' end)
	local message_content = NetworkContentElement()
	message_content['channel'] = channel
	message_content['content'] = content
	
	if send_message(proxy, true, client, message, message_content, instance_id) then 
		proxy.GLOBAL_INSTANCE_ID = proxy.GLOBAL_INSTANCE_ID + 1	end	
end
--
-- ProxyEntity * ClientEntity * string * string
-- send chat private message
function send_private_chat(proxy, client, receiver, content)
	local instance_id = proxy.GLOBAL_INSTANCE_ID + 1
	local message = get_message('ChatClientPrivateMessage')
	local message_content = NetworkContentElement()
	message_content['receiver'] = receiver
	message_content['content'] = content
	
	if send_message(proxy, true, client, message, message_content, instance_id) then
		proxy.GLOBAL_INSTANCE_ID = proxy.GLOBAL_INSTANCE_ID + 1 end
end
--
-- end game actions

--
-- end proxy fonctions --- 



-- general functions ---
-- for more information https://github.com/Mrpotatosse/AivyCore/blob/master/AivyDofus/LuaCode/CodeSleep.cs
-- double * FUNC -> ()																							-- wait double millisecondes then do FUNC
function sleep_then(value, on_end)
	return sleeper:sleep_and_continue(value, on_end)
end
-- double * FUNC -> ()																							-- async wait double millisecondes then do FUNC
function async_sleep_then(value, on_end)
	return sleeper:async_sleep_and_continue(value, on_end)
end
--
-- end general functions ---

-- MAIN PROGRAM -- 
-- set comment if you want to run different
config = get_config('remote_up') -- get config
proxy = start_proxy_from_config(config, 666) -- start proxy
accept_callback = multi_proxy[proxy.Port] -- get callback
async_sleep_then(2000, update_dofus2_protocol) -- update dofus2 protocol
-- END MAIN PROGRAM --

print('default lua code inited')

