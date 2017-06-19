; (function ($,fns) {
    fns.Config.Ajax =
        {
            beforeAjax: function (ajaxopts) {
                if (ajaxopts) {
                    //加入翻页器的数据
                    if ($("#toPage").size() > 0) {
                        ajaxopts.postdata["toPage"] = $("#toPage").val();
                    }
                    if ($("#Count_per_Page").size() > 0) {
                        ajaxopts.postdata["Count_per_Page"] = $("#Count_per_Page").val();
                    }
                }
            },
            //请求成功，并处理返回值
            successAction: function (ajaxopts, rtn) {
                //判斷jsonstr是否就是json對象
                var jobj = rtn;
                if (ajaxopts.returntype.toLowerCase() == "json") {
                    if (jobj.ErrorCode == "") {
                        if (jobj.Content.__isneedlogin__) {
                            fns.Message.DialogLogin("请先登录！", jobj.Content.__loginurl__);
                        } else {
                            if (ajaxopts.success != null) {
                                ajaxopts.success(jobj.Content);
                            }
                        }
                    } else {
                        if (jobj.ErrorCode == "AU01") {
                            fns.Message.DialogLogin(jobj.ErrorMsg, jobj.Content);
                        } else {
                            fns.ExceptionProcess.ShowErrorMsg(jobj.ErrorCode + "\n" + jobj.ErrorMsg);
                            if (ajaxopts.fail) {
                                ajaxopts.fail(jobj.ErrorCode, jobj.ErrorMsg);
                            }
                        }
                    }
                } else {
                    if (ajaxopts.success != null) {
                        ajaxopts.success(jobj);
                    }
                }

            }
        }
}(jQuery, FrameNameSpace));
