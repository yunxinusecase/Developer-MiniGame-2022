/** @file NIMMsgLogAPI.cs
  * @brief NIM SDK提供的消息历史接口 
  * @copyright (c) 2015, NetEase Inc. All rights reserved
  * @author Harrison
  * @date 2015/12/8
  */

using System;
using System.Diagnostics;
using NimUtility;
using NimUtility.Json;
using NIM.Messagelog.Delegate;
using NIM.Session;
using System.Collections.Generic;

namespace NIM.Messagelog
{
    public delegate void QueryLogByMsgIdResultDelegate(ResponseCode code, string msdId, NIMIMMessage msg);

    public delegate void QueryMsglogResultDelegate(ResponseCode code, string accountId, NIMSessionType sType, MsglogQueryResult result);

    public delegate void OperateMsglogResultDelegate(ResponseCode code, string uid, NIMSessionType sType);

    public delegate void OperateSingleLogResultDelegate(ResponseCode code, string msgId);

    public delegate void CommonOperationResultDelegate(ResponseCode code);

    public delegate void ImportProgressDelegate(long importedCount, long totalCount);

    public delegate void MsglogStatusChangedDelegate(ResponseCode res, string result);

    public delegate void UpdateLocalExtDelegate(ResponseCode res, string msgId);

    public class MessagelogAPI
    {
        private static readonly QuerySingleLogDelegate QuerySingleLogCompleted;
        private static readonly QueryMessageLogDelegate QueryLogCompleted;
        private static readonly OperateMsglogByObjectIdDelegate OperateMsglogByObjIdCompleted;
        private static readonly OperateMsglogByLogIdDelegate OperateMsglogByLogIdCompleted;
        private static readonly OperateMsglogCommonDelegate NormalOperationCompleted;
        private static readonly NimMsglogStatusChangedCbFunc OnMsglogStatusChanged;
        private static readonly NimMsglogStatusChangedCbFunc OnGlobalMsglogStatusChanged;
        private static readonly NimMsglogResCbFunc OnUpdateLocalExtCompleted;

        static MessagelogAPI()
        {
            QuerySingleLogCompleted = OnQuerySingleLogCompleted;
            QueryLogCompleted = OnQuerylogCompleted;
            OperateMsglogByObjIdCompleted = OnOperateMsglogByObjIdCompleted;
            OperateMsglogByLogIdCompleted = OnOperateMsglogByLogIdCompleted;
            NormalOperationCompleted = OnNormalOperationCompleted;
            OnMsglogStatusChanged = MsglogChangedCallback;
            OnGlobalMsglogStatusChanged = GlobalMsglogStatusChangedCallback;
            OnUpdateLocalExtCompleted = UpdateLocalExtCallback;
        }

        /// <summary>
        ///     根据消息ID查询本地（单条）消息
        /// </summary>
        /// <param name="clientMsgId"></param>
        /// <param name="action"></param>
        public static void QuerylogById(string clientMsgId, QueryLogByMsgIdResultDelegate action)
        {
            var ptr = DelegateConverter.ConvertToIntPtr(action);
            MsglogNativeMethods.nim_msglog_query_msg_by_id_async(clientMsgId, null, QuerySingleLogCompleted, ptr);
        }

        [MonoPInvokeCallback(typeof(QuerySingleLogDelegate))]
        private static void OnQuerySingleLogCompleted(int resCode, string msgId, string result, string jsonExtension, IntPtr userData)
        {
            if (userData == IntPtr.Zero)
                return;
            var msg = MessageFactory.CreateMessage(result);
            userData.InvokeOnce<QueryLogByMsgIdResultDelegate>((ResponseCode)resCode, msgId, msg);
        }

        /// <summary>
        ///     查询本地消息（按时间逆序起查，逆序排列）
        /// </summary>
        /// <param name="accountId">会话id，对方的account id或者群组tid</param>
        /// <param name="sType">会话类型</param>
        /// <param name="limit">一次查询数量，建议20</param>
        /// <param name="msgAnchorTimetag">上次查询最后一条消息的时间戳（按时间逆序起查，即最小的时间戳）</param>
        /// <param name="action"></param>
        public static void QueryMsglogLocally(string accountId, NIMSessionType sType, int limit, long msgAnchorTimetag, QueryMsglogResultDelegate action)
        {
            var ptr = DelegateConverter.ConvertToIntPtr(action);
            MsglogNativeMethods.nim_msglog_query_msg_async(accountId, sType, limit, msgAnchorTimetag, null, QueryLogCompleted, ptr);
        }
        /// <summary>
        /// 包含起止时间的历史消息查询
        /// </summary>
        /// <param name="args">查询参数</param>
        /// <param name="action">查询结果回调通知</param>
        public static void QueryMsglogLocally(QueryMsglogParams args, QueryMsglogResultDelegate action)
        {
            var x = new { direction = args.Direction, reverse = args.Reverse, endtime = args.EndTimetag };
            var json_ext = JsonParser.Serialize(x);
            var ptr = DelegateConverter.ConvertToIntPtr(action);
            MsglogNativeMethods.nim_msglog_query_msg_async(args.AccountId, args.SessionType, args.CountLimit, args.MsgAnchorTimttag, json_ext, QueryLogCompleted, ptr);
        }

        [MonoPInvokeCallback(typeof(QueryMessageLogDelegate))]
        private static void OnQuerylogCompleted(int resCode, string id, NIMSessionType type, string result, string jsonExtension, IntPtr userData)
        {
            if (userData == IntPtr.Zero)
                return;
            var queryResult = new MsglogQueryResult();
            queryResult.CreateFromJsonString(result);
            userData.InvokeOnce<QueryMsglogResultDelegate>((ResponseCode)resCode, id, type, queryResult);
        }

        /// <summary>
        ///     在线查询消息
        /// </summary>
        /// <param name="id">会话id，对方的account id或者群组tid</param>
        /// <param name="sType">会话类型</param>
        /// <param name="limit">本次查询的消息条数上限(最多100条)</param>
        /// <param name="sTimetag">起始时间点，单位：毫秒</param>
        /// <param name="eTimetag">结束时间点，单位：毫秒</param>
        /// <param name="endMsgId">结束查询的最后一条消息的server_msg_id(不包含在查询结果中) </param>
        /// <param name="reverse">true：反向查询(按时间正序起查，正序排列)，false：按时间逆序起查，逆序排列（建议默认为false）</param>
        /// <param name="saveLocal">true: 将在线查询结果保存到本地，false: 不保存</param>
        /// <param name="action"></param>
        public static void QueryMsglogOnline(string id, NIMSessionType sType, int limit, long sTimetag, long eTimetag,
            long endMsgId, bool reverse, bool saveLocal, QueryMsglogResultDelegate action)
        {
            var ptr = DelegateConverter.ConvertToIntPtr(action);
            MsglogNativeMethods.nim_msglog_query_msg_online_async(id, sType, limit, sTimetag, eTimetag, endMsgId, reverse, saveLocal, null, QueryLogCompleted, ptr);
        }

//#if NIMAPI_UNDER_WIN_DESKTOP_ONLY
        /// <summary>
        ///     在线查询消息
        /// </summary>
        /// <param name="id">会话id，对方的account id或者群组tid</param>
        /// <param name="sType">会话类型</param>
        /// <param name="limit">本次查询的消息条数上限(最多100条)</param>
        /// <param name="sTimetag">起始时间点，单位：毫秒</param>
        /// <param name="eTimetag">结束时间点，单位：毫秒</param>
        /// <param name="endMsgId">结束查询的最后一条消息的server_msg_id(不包含在查询结果中) </param>
        /// <param name="reverse">true：反向查询(按时间正序起查，正序排列)，false：按时间逆序起查，逆序排列（建议默认为false）</param>
        /// <param name="saveLocal">true: 将在线查询结果保存到本地，false: 不保存</param>
        /// <param name="autoDownloadAttach">查询结果回来后，是否需要sdk自动下载消息附件</param>
        /// <param name="action"></param>
        public static void QueryMsglogOnline(string id, NIMSessionType sType, int limit, long sTimetag, long eTimetag,
            long endMsgId, bool reverse, bool saveLocal,bool autoDownloadAttach, QueryMsglogResultDelegate action)
        {
            var ptr = DelegateConverter.ConvertToIntPtr(action);
            System.Collections.Generic.Dictionary<string, bool> dic = new System.Collections.Generic.Dictionary<string, bool>();
            dic[QueryMsglogParams.AutoDownloadAttachJsonKey] = autoDownloadAttach;
            var jsonExt = JsonParser.Serialize(dic);
            MsglogNativeMethods.nim_msglog_query_msg_online_async(id, sType, limit, sTimetag, eTimetag, endMsgId, reverse, saveLocal, jsonExt, QueryLogCompleted, ptr);
        }

//#endif
        /// <summary>
        ///     根据指定条件查询本地消息,使用此接口可以完成全局搜索等功能,具体请参阅开发手册 http://dev.netease.im/docs?doc=pc&#历史记录
        /// </summary>
        /// <param name="range">消息历史的检索范围</param>
        /// <param name="ids">会话id（对方的account id或者群组tid）的集合</param>
        /// <param name="limit">本次查询的消息条数上限(默认100条)</param>
        /// <param name="sTimetag">起始时间点，单位：毫秒</param>
        /// <param name="eTimetag">结束时间点，单位：毫秒</param>
        /// <param name="endMsgId">结束查询的最后一条消息的client_msg_id(不包含在查询结果中)（暂不启用）</param>
        /// <param name="reverse">true：反向查询(按时间正序起查，正序排列)，false：按时间逆序起查，逆序排列（建议默认为false）</param>
        /// <param name="msgType">检索的消息类型（目前只支持kNIMMessageTypeText、kNIMMessageTypeImage和kNIMMessageTypeFile这三种类型消息）</param>
        /// <param name="searchContent">
        ///     检索文本（目前只支持kNIMMessageTypeText和kNIMMessageTypeFile这两种类型消息的文本关键字检索，
        ///     即支持文字消息和文件名的检索。
        ///     如果合并检索，需使用未知类型消息kNIMMessageTypeUnknown）
        /// </param>
        /// <param name="action"></param>
        public static void QueryMsglogByCustomCondition(NIMMsgLogQueryRange range, string[] ids, int limit,
            long sTimetag, long eTimetag, string endMsgId, bool reverse,
            NIMMessageType msgType, string searchContent, QueryMsglogResultDelegate action)
        {
            var ptr = DelegateConverter.ConvertToIntPtr(action);
            var idJson = JsonParser.Serialize(ids);
            MsglogNativeMethods.nim_msglog_query_msg_by_options_async(range, idJson, limit, sTimetag, eTimetag, endMsgId,
                reverse, msgType, searchContent, null, QueryLogCompleted, ptr);
        }

        /// <summary>
        ///     批量设置未读状态为已读消息状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sType"></param>
        /// <param name="action"></param>
        public static void MarkMessagesStatusRead(string id, NIMSessionType sType, OperateMsglogResultDelegate action)
        {
            var ptr = DelegateConverter.ConvertToIntPtr(action);
            MsglogNativeMethods.nim_msglog_batch_status_read_async(id, sType, null, OperateMsglogByObjIdCompleted, ptr);
        }
        [MonoPInvokeCallback(typeof(OperateMsglogByObjectIdDelegate))]
        private static void OnOperateMsglogByObjIdCompleted(int resCode, string uid, NIMSessionType type, string jsonExtension, IntPtr userData)
        {
            userData.InvokeOnce<OperateMsglogResultDelegate>((ResponseCode)resCode, uid, type);
        }

        /// <summary>
        ///     批量删除指定对话的消息。删除成功后，将相应会话项的最后一条消息的状态kNIMSessionMsgStatus设置为已删除状态
        /// </summary>
        /// <param name="id">会话id，对方的account id或者群组tid</param>
        /// <param name="sType">会话类型</param>
        /// <param name="action"></param>
        public static void BatchDeleteMeglog(string id, NIMSessionType sType, OperateMsglogResultDelegate action)
        {
            var ptr = DelegateConverter.ConvertToIntPtr(action);
            MsglogNativeMethods.nim_msglog_batch_status_delete_async(id, sType, null, OperateMsglogByObjIdCompleted, ptr);
        }

        /// <summary>
        ///     设置消息状态
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="status"></param>
        /// <param name="action"></param>
        public static void SetMsglogStatus(string msgId, NIMMsgLogStatus status, OperateSingleLogResultDelegate action)
        {
            var ptr = DelegateConverter.ConvertToIntPtr(action);
            MsglogNativeMethods.nim_msglog_set_status_async(msgId, status, null, OperateMsglogByLogIdCompleted, ptr);
        }

        [MonoPInvokeCallback(typeof(OperateMsglogByLogIdDelegate))]
        private static void OnOperateMsglogByLogIdCompleted(int resCode, string msgId, string jsonExtension, IntPtr userData)
        {
            userData.InvokeOnce<OperateSingleLogResultDelegate>((ResponseCode)resCode, msgId);
        }

        /// <summary>
        ///     设置消息子状态
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="status"></param>
        /// <param name="action"></param>
        public static void SetMsglogSubStatus(string msgId, NIMMsgLogSubStatus status, OperateSingleLogResultDelegate action)
        {
            var ptr = DelegateConverter.ConvertToIntPtr(action);
            MsglogNativeMethods.nim_msglog_set_sub_status_async(msgId, status, null, OperateMsglogByLogIdCompleted, ptr);
        }

        /// <summary>
        ///     往本地消息历史数据库里写入一条消息（如果已存在这条消息，则更新。通常是APP的本地自定义消息，并不会发给服务器）
        /// </summary>
        /// <param name="uid">会话id，对方的account id或者群组tid</param>
        /// <param name="need_update_session">是否更新会话列表（一般最新一条消息会有更新的需求）</param>
        /// <param name="msg">消息体</param>
        /// <param name="action">操作结果的回调函数</param>
        public static void WriteMsglog(string uid, bool need_update_session, NIMIMMessage msg, OperateSingleLogResultDelegate action)
        {
            var ptr = DelegateConverter.ConvertToIntPtr(action);
            var msgJsonValue = msg.Serialize();
            MsglogNativeMethods.nim_msglog_insert_msglog_async(uid, msgJsonValue, need_update_session, null, OperateMsglogByLogIdCompleted, ptr);
        }

        /// <summary>
        ///     删除指定会话类型的所有消息
        /// </summary>
        /// <param name="sType">会话类型</param>
        /// <param name="deleteSessions">是否删除指定会话类型的所有会话列表项</param>
        /// <param name="action"></param>
        public static void DeleteMsglogsBySessionType(NIMSessionType sType, bool deleteSessions, OperateMsglogResultDelegate action)
        {
            var ptr = DelegateConverter.ConvertToIntPtr(action);
            MsglogNativeMethods.nim_msglog_delete_by_session_type_async(deleteSessions, sType, null, OperateMsglogByObjIdCompleted, ptr);
        }

        /// <summary>
        ///     删除指定一条消息
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="sType"></param>
        /// <param name="msgId"></param>
        /// <param name="action"></param>
        public static void DeleteSpecifiedMsglog(string sid, NIMSessionType sType, string msgId, OperateSingleLogResultDelegate action)
        {
            var ptr = DelegateConverter.ConvertToIntPtr(action);
            MsglogNativeMethods.nim_msglog_delete_async(sid, sType, msgId, null, OperateMsglogByLogIdCompleted, ptr);
        }

        /// <summary>
        ///     删除全部消息历史
        /// </summary>
        /// <param name="deleteSessions">是否删除所有会话列表项（即全部最近联系人）</param>
        /// <param name="action"></param>
        public static void ClearAll(bool deleteSessions, CommonOperationResultDelegate action)
        {
            var ptr = DelegateConverter.ConvertToIntPtr(action);
            MsglogNativeMethods.nim_msglog_delete_all_async(deleteSessions, null, NormalOperationCompleted, ptr);
        }

        [MonoPInvokeCallback(typeof(OperateMsglogCommonDelegate))]
        private static void OnNormalOperationCompleted(int resCode, string jsonExtension, IntPtr userData)
        {
            userData.InvokeOnce<CommonOperationResultDelegate>((ResponseCode)resCode);
        }

        /// <summary>
        ///     导出整个消息历史DB文件（不包括系统消息历史）
        ///     android 和 ios 平台下不可用
        /// </summary>
        /// <param name="destPath">导出时保存的目标全路径</param>
        /// <param name="action"></param>
        public static void ExportDatabaseFile(string destPath, CommonOperationResultDelegate action)
        {
            var ptr = DelegateConverter.ConvertToIntPtr(action);
            MsglogNativeMethods.nim_msglog_export_db_async(destPath, null, NormalOperationCompleted, ptr);
        }

        /// <summary>
        ///     导入消息历史DB文件（不包括系统消息历史）。先验证是否自己的消息历史文件和DB加密密钥，如果验证不通过，则不导入。
        ///     android 和 ios 平台下不可用
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="action"></param>
        /// <param name="prg"></param>
        public static void ImportDatabase(string srcPath, CommonOperationResultDelegate action, ImportProgressDelegate prg)
        {
            var ptr1 = DelegateConverter.ConvertToIntPtr(action);
            var ptr2 = DelegateConverter.ConvertToIntPtr(prg);

            MsglogNativeMethods.nim_msglog_import_db_async(srcPath, null, NormalOperationCompleted, ptr1, ImportMsglogPrgCb, ptr2);
        }

        private static readonly ImportMsglogProgressDelegate ImportMsglogPrgCb = ReportImportDbProgress;

        [MonoPInvokeCallback(typeof(ImportMsglogProgressDelegate))]
        private static void ReportImportDbProgress(long importedSize, long totalSize, string jsonExtension, IntPtr userData)
        {
            userData.Invoke<ImportProgressDelegate>(importedSize, totalSize);
        }

        /// <summary>
        ///     消息是否已经被查看
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="jsonExtension"></param>
        /// <returns></returns>
        public static bool IsMessageBeReaded(NIMIMMessage msg, string jsonExtension = null)
        {
            System.Diagnostics.Debug.Assert(msg != null && !string.IsNullOrEmpty(msg.ReceiverID));
            var msgJson = msg.Serialize();
            return MsglogNativeMethods.nim_msglog_query_be_readed(msgJson, jsonExtension);
        }

        /// <summary>
        ///     发送已读回执
        /// </summary>
        public static void SendReceipt(NIMIMMessage msg, MsglogStatusChangedDelegate cb, string jsonExtension = null)
        {
            System.Diagnostics.Debug.Assert(msg != null && !string.IsNullOrEmpty(msg.ReceiverID));
            var msgJson = msg.Serialize();
            var ptr = DelegateConverter.ConvertToIntPtr(cb);
            MsglogNativeMethods.nim_msglog_send_receipt_async(msgJson, jsonExtension, OnMsglogStatusChanged, ptr);
        }

        [MonoPInvokeCallback(typeof(NimMsglogStatusChangedCbFunc))]
        private static void MsglogChangedCallback(int res_code, string result, string json_extension, IntPtr userData)
        {
            userData.InvokeOnce<MsglogStatusChangedDelegate>((ResponseCode)res_code, result);
        }

        [MonoPInvokeCallback(typeof(NimMsglogStatusChangedCbFunc))]
        private static void GlobalMsglogStatusChangedCallback(int res_code, string result, string json_extension, IntPtr userData)
        {
            userData.Invoke<MsglogStatusChangedDelegate>((ResponseCode)res_code, result);
        }

        [MonoPInvokeCallback(typeof(NimMsglogResCbFunc))]
        private static void UpdateLocalExtCallback(int res_code, string msg_id, string json_extension, IntPtr userData)
        {
            userData.Invoke<UpdateLocalExtDelegate>((ResponseCode)res_code, msg_id);
        }

        /// <summary>
        ///     注册全局的消息状态变更通知（目前只支持已读状态的通知）
        /// </summary>
        /// <param name="cb"></param>
        public static void RegMsglogStatusChangedCb(MsglogStatusChangedDelegate cb)
        {
            var ptr = DelegateConverter.ConvertToIntPtr(cb);
            MsglogNativeMethods.nim_msglog_reg_status_changed_cb(null, OnGlobalMsglogStatusChanged, ptr);
        }

        /// <summary>
        ///     更新本地扩展字段内容
        /// </summary>
        /// <param name="msgId">消息Id</param>
        /// <param name="localExt">消息本地扩展字段内容</param>
        /// <param name="cb">操作结果的回调函数</param>
        public static void UpdateLocalExt(string msgId, string localExt, UpdateLocalExtDelegate cb)
        {
            var ptr = DelegateConverter.ConvertToIntPtr(cb);
            MsglogNativeMethods.nim_msglog_update_localext_async(msgId, localExt, null, OnUpdateLocalExtCompleted, ptr);
        }


#if NIMAPI_UNDER_WIN_DESKTOP_ONLY
        /// <summary>
        /// 全部未读消息历史标记为已读
        /// </summary>
        /// <param name="cb"></param>
        public static void ReadAll(CommonOperationResultDelegate cb)
        {
            var ptr = NimUtility.DelegateConverter.ConvertToIntPtr(cb);
            MsglogNativeMethods.nim_msglog_read_all_async(null, NormalOperationCompleted, ptr);
        }

        /// <summary>
        /// 根据指定条件在一个会话中查询指定单个或多个类型的本地消息
        /// </summary>
        /// <param name="sessionType">会话类型</param>
        /// <param name="id">会话id，对方的account id或者群组tid</param>
        /// <param name="limit">本次查询的消息条数上限(默认100条)</param>
        /// <param name="fromTime">起始时间点，单位：毫秒</param>
        /// <param name="endTime">结束时间点，单位：毫秒</param>
        /// <param name="endClientMsgId">结束查询的最后一条消息的client_msg_id(不包含在查询结果中)（暂不启用）</param>
        /// <param name="reverse">true：反向查询(按时间正序起查，正序排列)，false：按时间逆序起查，逆序排列（建议默认为false）</param>
        /// <param name="msgTypes">检索的消息类型</param>
        /// <param name="cb">本地查询消息的回调函数</param>
        /// <param name="jsonExt">json扩展参数（备用，目前不需要）</param>
        public static void QuerySpecifiedType(NIMSessionType sessionType,string id,int limit,long fromTime,
            long endTime,string endClientMsgId,bool reverse,List<NIMMessageType> msgTypes, QueryMsglogResultDelegate cb,string jsonExt = null)
        {
            var ptr = DelegateConverter.ConvertToIntPtr(cb);
            var msgTypeJson = JsonParser.Serialize(msgTypes);
            MsglogNativeMethods.nim_msglog_query_the_message_of_the_specified_type_async(sessionType,
                id, limit, fromTime, endTime, endClientMsgId, reverse, msgTypeJson, jsonExt, QueryLogCompleted, ptr);
        }

        /// <summary>
        /// 查询收到的消息是否已经发送过已读回执
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="jsonExt">json扩展参数（备用，目前不需要）</param>
        /// <returns></returns>
        public static bool IsMsgReceiptSended(NIMIMMessage msg,string jsonExt = null)
        {
            var msgJson = msg.Serialize();
            return MsglogNativeMethods.nim_msglog_query_receipt_sent(msgJson, jsonExt);
        }
#endif
    }
}