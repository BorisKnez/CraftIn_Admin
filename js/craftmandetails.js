var pot;
var strServerPath;
var domainName;
$(function () {
    $(".loadingBar").css("display", "none");
    pot = $("input[id$='hfControlPath']").val();
    strServerPath = $("input[id$='hfstrServerPath']").val();
    domainName = $("input[id$='hfDomainName']").val();
    //console.log(pot);
    //console.log(strServerPath);
    //console.log(domainName); 

    $('.dropdownCountry').change(function () {
        $.ajax({
            type: "POST",
            url: pot + "CraftManWS.asmx/getRegionsForCountry",
            dataType: "xml",
            data: {
                'country': $(".dropdownCountry").val()
            },
            success: function (result) {
                if (result != null) {
                    if ($('#craftRegions').next().hasClass("parsley-errors-list")) {
                        $('#craftRegions').next().remove();
                    }
                    $('#craftRegions').html($(result).text());
                }
            },
            error: function (request, status, error) {
                if (window.console) {
                    console.log("request: " + request + " status: " + status + " error: " + error);
                }
            }
        });
    });

    $('#LogoUpload').fileupload({
        replaceFileInput: false,
        forceIframeTransport: true,
        autoUpload: true,
        singleFileUploads: true,
        dataType: "xml",
        url: pot + "CraftManWS.asmx/updateCraftManPicture",
        add: function (e, data) {
            data.submit();
        },
        done: function (e, data) {
            console.log(data);
            updatedImageName = $(data.result).text();
            if (updatedImageName != "" && updatedImageName != 'undefined') {
                $("input[id$='hfImageName']").val(updatedImageName);
                var picURL = "/Portals/craftman_pictures/" + updatedImageName;
                $(".mw-image-look").html("<img style='width: auto; max-height: 170px; display: block;' src='" + picURL + "' alt='image'><input type='button' style='background: none repeat scroll 0px 0px rgb(190, 27, 44); color: white; width: auto; padding: 5px 10px; margin-bottom: 5px;' class='btnRemoveImage' value='Remove'>");
            }
        },
        fail: function (e, data) {
            console.log(e);
            console.log(data);
        }
    });

    $('body').on('click', '.btnRemoveImage', function () {
        $(".mw-image-look").html("");
        $('#LogoUpload').replaceWith($('#LogoUpload').val('').clone(true));
        $("input[id$='hfImageName']").val("");
    })



});



function validateInputs() {
    var result;
    if (result != null)
    {
        result.reset();
    }
    result = $('#craftContent').parsley();

    console.log(result.isValid());
    if (result.isValid()) {
        addNewCraftMan();
    }
}

function validateInputsEdit() {
    var result;
    if (result != null) {
        result.reset();
    }
    result = $('#craftContent').parsley();

    if (result.isValid()) {
        editCraftMan();
    }
}


function addNewCraftMan() {
    $(".loadingBar").css("display", "inline");
    $(".btnAddNewCraftman").prop('disabled', true);
    $(".btnAddNewCraftman").css("display", "none");

    var checkedValuesRegions = $("input[name^='craftRegions']:checked").map(function () {
        return this.value;
    }).get();

    var checkedValuesCategories = $("input[name^='craftCategories']:checked").map(function () {
        return this.value;
    }).get();


    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        url: pot + "CraftManWS.asmx/CreateNewPortal",
        data: JSON.stringify({
            "strServer": strServerPath,
            "domain": domainName,
            "strPortalName": $("input[id$='txtPortalAlias']").val(),
            "strPassword": $("input[id$='txtPassword']").val(),
            "strEmail": $("input[id$='txtEmailAddress']").val(),
            "strFirstName": $("input[id$='txtFirstName']").val(),
            "strLastName": $("input[id$='txtLastName']").val(),
            "strUserName": $("input[id$='txtUsername']").val(),
            "strBusinessName": $("input[id$='txtBusinesssName']").val(),
            "strBusinessDescription": $("textarea[id$='txtDescription']").val(),
            "strBusinessEmail": $("input[id$='txtBusinessEmailAddress']").val(),
            "strBusinessTelephone": $("input[id$='txtTelephone']").val(),
            "strBusinessAddress": $("input[id$='txtAddress']").val(),
            "strBusinessCity": $("input[id$='txtCity']").val(),
            "strBusinessZipCode": $("input[id$='txtZipCode']").val(),
            "strBusinessRegions": checkedValuesRegions.toString(),
            "strBusinessCategories": checkedValuesCategories.toString(),
            "strBusinessVatID": $("input[id$='txtVatID']").val(),
            "strCountryID": $("select[id$='dropdownCountry']").val(),
            "selectedTemplate": $("select[id$='dropdownTemplate']").val(),
            "strBusinessMainImage": $("input[id$='hfImageName']").val()
        }),
        success: function (result) {
            if (result.hasOwnProperty("d"))
            {
                if (result.d[0].Status == 1)
                {
                    parent.$("input[id$='hfMessageFromCB']").val("OK");
                    $.colorbox.close();
                }
                else if (result.d[0].Status == 2)
                {
                    $(".loadingBar").css("display", "none");
                    $(".btnAddNewCraftman").prop('disabled', false);
                    $(".btnAddNewCraftman").css("display", "block");
                    alert("Missing data");
                }
                else if (result.d[0].Status == 3) {
                    $(".loadingBar").css("display", "none");
                    $(".btnAddNewCraftman").prop('disabled', false);
                    $(".btnAddNewCraftman").css("display", "block");
                    alert("Error" + result.d[0].Message);
                }
            }
        },
        error: function (request, status, error) {
            if (window.console) {
                console.log("request: " + request + " status: " + status + " error: " + error);
            }
        }
    });
}



function editCraftMan() {
    $(".loadingBar").css("display", "inline");
    $(".btnEditCraftMan").prop('disabled', true);
    $(".btnEditCraftMan").css("display", "none");

    var checkedValuesRegions = $("input[name^='craftRegions']:checked").map(function () {
        return this.value;
    }).get();

    var checkedValuesCategories = $("input[name^='craftCategories']:checked").map(function () {
        return this.value;
    }).get();

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        url: pot + "CraftManWS.asmx/EditCraftman",
        data: JSON.stringify({
            "craftManID": $("input[id$='hfCraftManID']").val(),
            "strEmail": $("input[id$='txtEmailAddress']").val(),
            "strFirstName": $("input[id$='txtFirstName']").val(),
            "strLastName": $("input[id$='txtLastName']").val(),
            "strBusinessName": $("input[id$='txtBusinesssName']").val(),
            "strBusinessDescription": $("textarea[id$='txtDescription']").val(),
            "strBusinessEmail": $("input[id$='txtBusinessEmailAddress']").val(),
            "strBusinessTelephone": $("input[id$='txtTelephone']").val(),
            "strBusinessAddress": $("input[id$='txtAddress']").val(),
            "strBusinessCity": $("input[id$='txtCity']").val(),
            "strBusinessZipCode": $("input[id$='txtZipCode']").val(),
            "strBusinessRegions": checkedValuesRegions.toString(),
            "strBusinessCategories": checkedValuesCategories.toString(),
            "strBusinessVatID": $("input[id$='txtVatID']").val(),
            "strCountryID": $("select[id$='dropdownCountry']").val(),
            "strBusinessMainImage": $("input[id$='hfImageName']").val()
        }),
        success: function (result) {
            if (result.hasOwnProperty("d")) {
                if (result.d[0].Status == 1) {
                    parent.$("input[id$='hfMessageFromCB']").val("EDITED");
                    $.colorbox.close();
                }
                else if (result.d[0].Status == 3) { /*error*/
                    $(".loadingBar").css("display", "none");
                    $(".btnEditCraftMan").prop('disabled', false);
                    $(".btnEditCraftMan").css("display", "block");
                    alert("Error" + result.d[0].Message);
                }
            }
        },
        error: function (request, status, error) {
            if (window.console) {
                console.log("request: " + request + " status: " + status + " error: " + error);
            }
        }
    });
}