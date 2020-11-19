--character = {
--	context = 0,
--	mapKey = '',
--	gameMapInfo = GameMapData()
--}
-- HANDLER FORM --

-- AbstractClientReceiveCallback * NetworkElement * NetworkContentElement -> bool
-- function your_handler_function(AbstractClientReceiveCallback, NetworkElement, NetworkContentElement) 				-- creating a handler function
--		return true 																									-- return true if message handled will be forwarded else false (if false you'll have to send the msg manualy)
-- end
-- handler_id = handlers:Add('YourMessageHandlerName', your_handler_function)										 	-- adding handler to handlers list (use handler_id to remove from handlers)

-- END HANDLER FORM --

-- AbstractClientReceiveCallback * NetworkElement * NetworkContentElement -> bool 										-- return forwarding rcv data 
-- same as c# handler , check README and c# source code for more information
--function protocol_required(callback, message, message_content)
	--print('from lua : '..message.BasicString)
	--print(message_content)
	--return true
--end

--function hello_connect_message(callback, message, message_content)
	--print('from lua : '..message.BasicString)
	--return true
--end

--function game_context_create_message(callback, message, message_content) -- from server
	--print('from lua : '..message.BasicString)
	--character.context = message_content['context']
	--return true
--end

--function map_complementary_informations_data_message(callback, message, message_content)
	--print('from lua : '..message.BasicString)
	--character.gameMapInfo.MapId = message_content['mapId']
	--character.gameMapInfo:FromMap(message_content)
	
	--local i = 0
	--while i < message_content['actors'].Length do
		--local actor = message_content['actors'][i]
		--local cellId = actor['disposition']['cellId']
		--local actorId = actor['contextualId']
	
		--local actor_data = ActorData()
		--actor_data.Id = actorId
	
		--character.gameMapInfo:AddActor(cellId, actor_data)
		--i = i + 1
	--end
	
	--visualizer:GetVisualizer(proxy.Port):FromMap(character.gameMapInfo.CurrentMap)
	--return true
--end

--function game_role_play_show_actor_message(callback, message, message_content)
	--print('from lua : '..message.BasicString)
	--local actor = message_content['informations']
	--local cellId = actor['disposition']['cellId']
	--local actorId = actor['contextualId']
	
	--local actor_data = ActorData()
	--actor_data.Id = actorId
	
	--character.gameMapInfo:AddActor(cellId, actor_data)
	--return true
--end

--function game_context_remove_element_message(callback, message, message_content)
	--print('from lua : '..message.BasicString)
	--character.gameMapInfo:Remove(message_content['id'])
	--return true
--end

--function game_map_movement_message(callback, message, message_content) -- from server
	--print('from lua '..message.BasicString)
	
	--local actorId = message_content['actorId']
	--local keys = message_content['keyMovements']
	
	--local cstart = keys[0] & 4095
	--local cend = keys[keys.Length - 1] & 4095
	
	--local actor_data = ActorData()
	--actor_data.Id = actorId
	
	--character.gameMapInfo:Remove(actorId)
	--character.gameMapInfo:AddActor(cend, actor_data)
	--return true
--end

--function game_map_movement_request_message(callback, message, message_content) -- from client
	--print('from lua : '..message.BasicString)
	
	--local keys = message_content['keyMovements']
	
	--local cstart = keys[0] & 4095
	--local cend = keys[keys.Length - 1] & 4095
	
	--character.gameMapInfo.MapId = message_content['mapId']
	--local path = maps:CompressedPath(character.gameMapInfo, cstart, cend, character.context == 2, false)
	
	--visualizer:GetVisualizer(proxy.Port):FromPath(path)
	--return true
--end	

--id_protocol_required_handler = proxy_handlers:Add('ProtocolRequired', protocol_required)
--id_hello_connect_message_handler = proxy_handlers:Add('HelloConnectMessage', hello_connect_message)
--id_map_complementary_informations_data_message_handler = proxy_handlers:Add('MapComplementaryInformationsDataMessage', map_complementary_informations_data_message)
--id_game_map_movement_request_message_handler = proxy_handlers:Add('GameMapMovementRequestMessage', game_map_movement_request_message)
--id_game_context_create_message_handler = proxy_handlers:Add('GameContextCreateMessage', game_context_create_message)
--id_game_role_play_show_actor_message_handler = proxy_handlers:Add('GameRolePlayShowActorMessage', game_role_play_show_actor_message)
--id_game_context_remove_element_message_handler = proxy_handlers:Add('GameContextRemoveElementMessage', game_context_remove_element_message)
--id_game_map_movement_message_handler = proxy_handlers:Add('GameMapMovementMessage', game_map_movement_message)
--print('default lua handler initied')