/*
** nopCommerce custom js functions
*/

/*
** nopCommerce ajax cart implementation
*/


var AjaxCart = {
    loadWaiting: false,
    usepopupnotifications: false,
    topcartselector: '',
    topwishlistselector: '',
    flyoutcartselector: '',

    init: function (usepopupnotifications, topcartselector, topwishlistselector, flyoutcartselector) {
        this.loadWaiting = false;
        this.usepopupnotifications = usepopupnotifications;
        this.topcartselector = topcartselector;
        this.topwishlistselector = topwishlistselector;
        this.flyoutcartselector = flyoutcartselector;
    },

    setLoadWaiting: function (display) {
        displayAjaxLoading(display);
        this.loadWaiting = display;
    },


    addproducttocart_catalog: function (urladd) {
        if (this.loadWaiting != false) {
            return;
        }
        this.setLoadWaiting(true);

        $.ajax({
            cache: false,
            url: urladd,
            type: 'post',
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },

    weightSender_productBox: function (productIdToSend) {

        console.log(productIdToSend);
        addproducttocart_catalog_Weight(productIdToSend);
    },

    //add a product to the cart/wishlist from the catalog pages
    addproducttocart_catalog_Weight: function (productId,url) {
       
        if (this.loadWaiting != false) {
            return;
        }
        this.setLoadWaiting(true);
        var Id4Jq = "#" + productId.toString();
        var decQuantity = parseFloat(document.getElementById(productId).innerHTML);
        var intProductId = parseInt(productId.split("-")[1]);
        var postData = {
            productId: intProductId,
            shoppingCartTypeId: 1,
            decQuantity: decQuantity
        };
       
        $.ajax({
            cache: false,
            url: url,
            type: 'post',
            data: postData,
            dataType: 'json',
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },


 
    increaseQuantityInput: function  (weight, prodId){
var idString = '#'+ prodId;
var currentValue = +$(idString).val();
                
var newValue = currentValue + parseInt(weight);
console.log(currentValue)
$(idString).val(newValue);

},

reduceQuantityInput: function  (weight, prodId){
    var idString = '#'+ prodId;
    var currentValue = +$(idString).val();
    if (currentValue > 0){
                    
        var newValue = currentValue - parseInt(weight);
        $(idString).val(newValue);
        
    }
},

    
    
    increaseQuantityFlyOutCart:  function (sciID,weight) {
        var idString = '#' + sciID;
        console.log("old weighy" + weight)
     var currentValue = +$(idString).html();
     var newValue = currentValue + parseInt(weight);
     console.log("new weighy" + newValue)
     $(idString).html(newValue);
     this.addproducttocart_flyOutCart_Weight(sciID, newValue);
},


 
 
    reduceQuantityFlyOutCart: function (sciID, weight) {
    var idString = '#' + sciID;
    var currentValue = +$(idString).html();
    if (currentValue > 0){
        console.log("old cart" + weight)
        var newValue = currentValue - parseInt(weight);
        //$(idString).html(newValue);
        console.log("old cart" + newValue)
        this.addproducttocart_flyOutCart_Weight(sciID, newValue);
    }
    },


    increaseQuantityCart: function (sciID, weight) {
        var idString = '#' + sciID;
        console.log("old cart" + weight)
        var currentValue = +$(idString).html();
        var newValue = currentValue + parseInt(weight);
        console.log("old cart" + newValue)
        $(idString).html(newValue);
       // this.addproducttocart_flyOutCart_Weight(sciID, newValue);
    },




    reduceQuantityCart: function (sciID, weight) {
        var idString = '#' + sciID;
        var currentValue = +$(idString).html();
        if (currentValue > 0) {
            console.log("old weighy" + weight)
            var newValue = currentValue - parseInt(weight);
            //$(idString).html(newValue);
            console.log("new weighy" + newValue)
            $(idString).value(newValue);
           // this.addproducttocart_flyOutCart_Weight(sciID, newValue);
        }
    },




    addproducttocart_flyOutCart_Weight: function (sciId,weight) {

        if (this.loadWaiting != false) {
            return;
        }
       // this.setLoadWaiting(true);
        //var Id4Jq = "#" + productId.toString();
        var decQuantity = parseFloat(weight);
        var intSciId = parseInt(sciId.split("-")[1]);
        var postData = {
            sciIdfromClient: intSciId,
            shoppingCartTypeId: 1,
            decQuantity: decQuantity
        };
        console.log(postData)
        $.ajax({
            cache: false,
            url: '/ShoppingCart/AddProductToCart_FlyOutCart_Weight',
            type: 'post',
            data: postData,
            dataType: 'json',
            //success: this.success_process,
            //complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },



    //addproducttocart_Cart_Weight: function (sciId, weight) {

    //    if (this.loadWaiting != false) {
    //        return;
    //    }
    //    this.setLoadWaiting(true);
    //    //var Id4Jq = "#" + productId.toString();
    //    var decQuantity = parseFloat(weight);
    //    var intSciId = parseInt(sciId.split("-")[1]);
    //    var postData = {
    //        sciIdfromClient: intSciId,
    //        shoppingCartTypeId: 1,
    //        decQuantity: decQuantity
    //    };
    //    console.log(postData)
    //    $.ajax({
    //        cache: false,
    //        url: '/ShoppingCart/AddProductToCart_FlyOutCart_Weight',
    //        type: 'post',
    //        data: postData,
    //        dataType: 'json',
    //        success: this.success_process,
    //        complete: this.resetLoadWaiting,
    //        error: this.ajaxFailure
    //    });
    //},

    //add a product to the cart/wishlist from the product details page
    addproducttocart_details: function (urladd, formselector) {
        if (this.loadWaiting != false) {
            return;
        }
        this.setLoadWaiting(true);

        $.ajax({
            cache: false,
            url: urladd,
            data: $(formselector).serialize(),
            type: 'post',
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },

    //add a product to compare list
    addproducttocomparelist: function (urladd) {
        if (this.loadWaiting != false) {
            return;
        }
        this.setLoadWaiting(true);

        $.ajax({
            cache: false,
            url: urladd,
            type: 'post',
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },

    success_process: function (response) {
        if (response.updatetopcartsectionhtml) {
            $(AjaxCart.topcartselector).html(response.updatetopcartsectionhtml);
        }
        if (response.updatetopwishlistsectionhtml) {
            $(AjaxCart.topwishlistselector).html(response.updatetopwishlistsectionhtml);
        }
        if (response.updateflyoutcartsectionhtml) {
            $(AjaxCart.flyoutcartselector).replaceWith(response.updateflyoutcartsectionhtml);
        }
        if (response.message) {
            //display notification
            if (response.success == true) {
                //success
                if (AjaxCart.usepopupnotifications == true) {
                    displayPopupNotification(response.message, 'success', true);
                }
                else {
                    //specify timeout for success messages
                    displayBarNotification(response.message, 'success', 3500);
                }
            }
            else {
                //error
                if (AjaxCart.usepopupnotifications == true) {
                    displayPopupNotification(response.message, 'error', true);
                }
                else {
                    //no timeout for errors
                    displayBarNotification(response.message, 'error', 0);
                }
            }
            return false;
        }
        if (response.redirect) {
            location.href = response.redirect;
            return true;
        }
        return false;
    },

    resetLoadWaiting: function () {
        AjaxCart.setLoadWaiting(false);
    },

    ajaxFailure: function () {
        alert('Failed to add the product. Please refresh the page and try one more time.');
    }
};

function OpenWindow(query, w, h, scroll) {
    var l = (screen.width - w) / 2;
    var t = (screen.height - h) / 2;

    winprops = 'resizable=0, height=' + h + ',width=' + w + ',top=' + t + ',left=' + l + 'w';
    if (scroll) winprops += ',scrollbars=1';
    var f = window.open(query, "_blank", winprops);
}

function setLocation(url) {
    window.location.href = url;
}

function displayAjaxLoading(display) {
    if (display) {
        $('.ajax-loading-block-window').show();
    }
    else {
        $('.ajax-loading-block-window').hide('slow');
    }
}

function displayPopupNotification(message, messagetype, modal) {
    //types: success, error, warning
    var container;
    if (messagetype == 'success') {
        //success
        container = $('#dialog-notifications-success');
    }
    else if (messagetype == 'error') {
        //error
        container = $('#dialog-notifications-error');
    }
    else if (messagetype == 'warning') {
        //warning
        container = $('#dialog-notifications-warning');
    }
    else {
        //other
        container = $('#dialog-notifications-success');
    }

    //we do not encode displayed message
    var htmlcode = '';
    if ((typeof message) == 'string') {
        htmlcode = '<p>' + message + '</p>';
    } else {
        for (var i = 0; i < message.length; i++) {
            htmlcode = htmlcode + '<p>' + message[i] + '</p>';
        }
    }

    container.html(htmlcode);

    var isModal = (modal ? true : false);
    container.dialog({
        modal: isModal,
        width: 350
    });
}
function displayPopupContentFromUrl(url, title, modal, width) {
    var isModal = (modal ? true : false);
    var targetWidth = (width ? width : 550);
    var maxHeight = $(window).height() - 20;

    $('<div></div>').load(url)
        .dialog({
            modal: isModal,
            position: ['center', 20],
            width: targetWidth,
            maxHeight: maxHeight,
            title: title,
            close: function(event, ui) {
                $(this).dialog('destroy').remove();
            }
        });
}

var barNotificationTimeout;
function displayBarNotification(message, messagetype, timeout) {
    clearTimeout(barNotificationTimeout);

    //types: success, error, warning
    var cssclass = 'success';
    if (messagetype == 'success') {
        cssclass = 'success';
    }
    else if (messagetype == 'error') {
        cssclass = 'error';
    }
    else if (messagetype == 'warning') {
        cssclass = 'warning';
    }
    //remove previous CSS classes and notifications
    $('#bar-notification')
        .removeClass('success')
        .removeClass('error')
        .removeClass('warning');
    $('#bar-notification .content').remove();

    //we do not encode displayed message

    //add new notifications
    var htmlcode = '';
    if ((typeof message) == 'string') {
        htmlcode = '<p class="content">' + message + '</p>';
    } else {
        for (var i = 0; i < message.length; i++) {
            htmlcode = htmlcode + '<p class="content">' + message[i] + '</p>';
        }
    }
    $('#bar-notification').append(htmlcode)
        .addClass(cssclass)
        .fadeIn('slow')
        .mouseenter(function ()
            {
                clearTimeout(barNotificationTimeout);
            });

    $('#bar-notification .close').unbind('click').click(function () {
        $('#bar-notification').fadeOut('slow');
    });

    //timeout (if set)
    if (timeout > 0) {
        barNotificationTimeout = setTimeout(function () {
            $('#bar-notification').fadeOut('slow');
        }, timeout);
    }
}

function htmlEncode(value) {
    return $('<div/>').text(value).html();
}

function htmlDecode(value) {
    return $('<div/>').html(value).text();
}


// CSRF (XSRF) security
function addAntiForgeryToken(data) {
    //if the object is undefined, create a new one.
    if (!data) {
        data = {};
    }
    //add token
    var tokenInput = $('input[name=__RequestVerificationToken]');
    if (tokenInput.length) {
        data.__RequestVerificationToken = tokenInput.val();
    }
    return data;
};