var pot;
var portalID;

$(window).resize(function () {
    $.colorbox.resize({
        width: window.innerWidth > parseInt(cboxOptions.maxWidth) ? cboxOptions.maxWidth : cboxOptions.width,
        height: window.innerHeight > parseInt(cboxOptions.maxHeight) ? cboxOptions.maxHeight : cboxOptions.height
    });
});

var cboxOptions = {
    width: '90%',
    height: '95%',
    maxWidth: '90%',
    maxHeight: '95%',
}

$(function () {


    pot = $("input[id$='hfControlPath']").val();
    portalID = $("input[id$='hfPortalID']").val();

    $("body").on('click', '.viewCurrentCraftMan', function () {
        var $parentTR = $(this).parent().parent();
        //console.log($parentTR.find("input[id^='hfCraftMan']").val());
        $.colorbox({
            href: pot + "craftManPage.aspx",
            width: '90%',
            height: '90%',
            maxWidth: '90%',
            overlayClose: false,
            data: {
                id_craft: $parentTR.find("input[id^='hfCraftMan']").val(),
                formType: 'view',
                controlPath: pot
            }
        });
    });


    $("body").on('click', '.editCurrentCraftMan', function () {
        var $parentTR = $(this).parent().parent();
        //console.log($parentTR.find("input[id^='hfCraftMan']").val());
        $.colorbox({
            href: pot + "craftManPage.aspx",
            width: '90%',
            height: '90%',
            maxWidth: '90%',
            overlayClose: false,
            data: {
                id_craft: $parentTR.find("input[id^='hfCraftMan']").val(),
                formType: 'edit',
                controlPath: pot
            },
            onClosed: function () {
                if ($("input[id$='hfMessageFromCB']").val().indexOf('EDITED') > -1) {
                    $(".craftMenListTR").html("<img src=\"" + pot + "/images/loading.gif\" />");
                    $.ajax({
                        url: pot + "CraftManWS.asmx/regenerateTable",
                        type: 'POST',
                        dataType: 'html',
                        data: {
                            'categoryID': $("select[id$='admincategory']").val(),
                            'RegionID': $("select[id$='adminregion']").val(),
                            'CountryID': $("select[id$='admincountry']").val()
                        },
                        success: function (result) {
                            if (result != null && result != undefined) {
                                $(".craftMenListTR").hide();
                                $(".craftMenListTR").html($(result).text()).fadeIn("slow");
                            }
                        },
                        error: function (error) {
                            alert(error);
                        }

                    });
                }
            }
        });
    });


    $('.button-addnewCraftman').click(function () {
        $.colorbox({
            href: pot + "craftManPage.aspx",
            width: '90%',
            height: '95%',
            maxWidth: '90%',
            overlayClose: false,
            data: {
                id_portal: portalID,
                formType: 'new',
                controlPath: pot
            },
            onClosed: function () {
                if ($("input[id$='hfMessageFromCB']").val().indexOf('OK') > -1) {
                    $(".craftMenListTR").html("<img src=\"" + pot + "/images/loading.gif\" />");
                    $.ajax({
                        url: pot + "CraftManWS.asmx/regenerateTable",
                        type: 'POST',
                        dataType: 'html',
                        data: {
                            'categoryID': $("select[id$='admincategory']").val(),
                            'RegionID': $("select[id$='adminregion']").val(),
                            'CountryID': $("select[id$='admincountry']").val()
                        },
                        success: function (result) {
                            if (result != null && result != undefined) {
                                $(".craftMenListTR").hide();
                                $(".craftMenListTR").html($(result).text()).fadeIn("slow");
                            }
                        },
                        error: function (error) {
                            alert(error);
                        }

                    });
                }

            }
        });
    });



    $("select[id$='adminregion']").change(function () {
        var val = $(this).val();
        if(val != null && val != 'undefined')
        {
            $(".craftMenListTR").html("<img src=\"" + pot + "/images/loading.gif\" />");
            $.ajax({
                url: pot + "CraftManWS.asmx/regenerateTable",
                type: 'POST',
                dataType: 'html',
                data: {
                    'categoryID': $("select[id$='admincategory']").val(),
                    'RegionID': val,
                    'CountryID': $("select[id$='admincountry']").val()
                },
                success: function (result) {
                    if (result != null && result != undefined) {
                        $(".craftMenListTR").hide();
                        $(".craftMenListTR").html($(result).text()).fadeIn("slow");
                    }
                },
                error: function (error) {
                    alert(error);
                }

            });
        }
    });

    $("select[id$='admincategory']").change(function () {
        var val = $(this).val();
        if (val != null && val != 'undefined') {
            $(".craftMenListTR").html("<img src=\"" + pot + "/images/loading.gif\" />");
            $.ajax({
                url: pot + "CraftManWS.asmx/regenerateTable",
                type: 'POST',
                dataType: 'html',
                data: {
                    'categoryID': val,
                    'RegionID': $("select[id$='adminregion']").val(),
                    'CountryID': $("select[id$='admincountry']").val()
                },
                success: function (result) {
                    if (result != null && result != undefined) {
                        $(".craftMenListTR").hide();
                        $(".craftMenListTR").html($(result).text()).fadeIn("slow");
                    }
                },
                error: function (error) {
                    alert(error);
                }

            });
        }
    });
    $("select[id$='admincountry']").change(function () {
        var val = $(this).val();
        if (val != null && val != 'undefined') {
            $(".craftMenListTR").html("<img src=\"" + pot + "/images/loading.gif\" />");
            $.ajax({
                url: pot + "CraftManWS.asmx/regenerateTable",
                type: 'POST',
                dataType: 'html',
                data: {
                    'categoryID': $("select[id$='admincategory']").val(),
                    'RegionID': $("select[id$='adminregion']").val(),
                    'CountryID': val
                },
                success: function (result) {
                    if (result != null && result != undefined) {
                        $(".craftMenListTR").hide();
                        $(".craftMenListTR").html($(result).text()).fadeIn("slow");
                    }
                },
                error: function (error) {
                    alert(error);
                }

            });
        }
    });


});

$(document).bind('cbox_complete', function () {
    $("#colorbox, #cboxOverlay").appendTo('form:first');
});