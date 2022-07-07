////const { search } = require("modernizr");
$(document).ready(function () {
    $('.modaldetail').click(function (e) {
        e.preventDefault();

        let url = $(this).attr('href');
        console.log(url);

        fetch(url).then(response => {
            return response.text();
        }).then(data => {
            $('.modal-content').html(data);

            $('.quick-view-image').slick({
                slidesToShow: 1,
                slidesToScroll: 1,
                arrows: false,
                dots: false,
                fade: true,
                asNavFor: '.quick-view-thumb',
                speed: 400,
            });

            $('.quick-view-thumb').slick({
                slidesToShow: 4,
                slidesToScroll: 1,
                asNavFor: '.quick-view-image',
                dots: false,
                arrows: false,
                focusOnSelect: true,
                speed: 400,
            });
        })
    })

    //$('.searchinput').keyup(function () {
    //    let search = $(this).val();
    //    console.log(search);

    //    let url = $(this).data("url");
    //    console.log(url);

    //    url = url + '?search=' + search
    //    console.log(url);

    //    fetch(url).then(res => {
    //        //console.log(response.json())
    //        return res.json()
    //    }).then(data =>
    //    {
    //        let listitemts = '';

    //        console.log(data.products.length)

    //        for (var i = 0; i < data.products.length; i++) {
    //            let liitem = `<li class="list-group-item">
    //            <img src="assets/images/product/${data.products[i].mainImage}" style="width:100px" alt="Alternate Text" />
    //            ${data.products[i].name}
    //        </li>`

    //            listitemts += liitem
    //        }

    //        console.log(listitemts)

    //        $('.search-body .list-group').html(listitemts)
    //    })
    //})


    //---------------search

    $('.searchinput').keyup(function () {
        let inputvalue = $(this).val().trim();
        console.log(inputvalue);

        let url = $(this).data('url');
        console.log(url);

        url = url + '?search=' + inputvalue;
        console.log(url);
        //input value bize true false deyeri qaytara biler -- boshdusa!!!
        //if (inputvalue) {
        //    console.log('salam');
        //}
        if (inputvalue) {
            fetch(url)
                .then(res => res.text())
                .then(data => {
                    $('.search-body .list-group').html(data);
                })
        }
        else {
            $('.search-body .list-group').html('');
        }
    })

    //-----------Add To Bask ------
    $(document).on('click','.addbasket',function (e) {
        e.preventDefault();

        let url = $(this).attr('href');

        fetch(url)
            .then(res => res.text())
            .then(data =>
            {
                $('.header-cart').html(data);
            });
    })

    $(document).on('click', '.deletefrombasket', function (e) {
        e.preventDefault();

        let url = $(this).attr('href');

        fetch(url)
            .then(res => res.text())
            .then(data =>
            {
                $('.header-cart').html(data);
            })
    })

    $(document).on('click', '.subCount', function (e) {
        e.preventDefault();
        let inputCount = $(this).next().val();

        if (inputCount >= 2) {
            inputCount--;
            $(this).next().val(inputCount);
            let url = $(this).attr('href') + '/?count=' + inputCount;
            console.log('sub');
            fetch(url)
                .then(res => res.text())
                .then(data => {
                    $('.basketindexcontainer').html(data);
                    fetch('/basket/getbasket')
                        .then(res => res.text())
                        .then(data => {
                            $('.header-cart').html(data);
                        });
                });
        }
    })

    $(document).on('click', '.addCount', function (e) {
        e.preventDefault();
        let inputCount = $(this).prev().val();
        
        if (inputCount > 0) {
            inputCount++;
        } else {
            inputCount = 1;
        }

        $(this).prev().val(inputCount);

        let url = $(this).attr('href') + '/?count=' + inputCount;
        console.log('add');
        fetch(url)
            .then(res => res.text())
            .then(data => {
                $('.basketindexcontainer').html(data);
                fetch('/basket/getbasket')
                    .then(res => res.text())
                    .then(data => {
                        $('.header-cart').html(data);
                    });
            });
    })

    $(document).on('click', '.deletefromcartbtn', function (e) {
        e.preventDefault();

        fetch($(this).attr('href'))
            .then(res => res.text())
            .then(data => {
                $('.basketindexcontainer').html(data);
                fetch('/basket/getbasket')
                    .then(res => res.text())
                    .then(data => {
                        $('.header-cart').html(data);
                    });
            })
    })

    let path = window.location.pathname
    path = path.split('/')
    console.log(path);
    let links = $('.header-horizontal-menu .menu-content li')

    console.log(links)

    for (var i = 0; i < links.length; i++) {
        let hrefpath = links[i].children[0].getAttribute('href').split('/')
        if (hrefpath[1].toLowerCase() == path[1].toLowerCase())
        {
            links[i].classList.add('active')
        } else {
            links[i].classList.remove('active')
        }
    }

    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": false,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }

    if ($('#successInput') != null) {
        toastr["success"]($('#successInput').val(), $('#successInput').val().split(' ')[0])
    }

    if ($('#successInputPassword') != null) {
        toastr["success"]($('#successInputPassword').val(), $('#successInputPassword').val().split(' ')[0])
    }

    $(document).on('show.bs.collapse','.accordian-body', function () {
        $(this).closest("table")
            .find(".collapse.in")
            .not(this)
        //.collapse('toggle')
    })
})