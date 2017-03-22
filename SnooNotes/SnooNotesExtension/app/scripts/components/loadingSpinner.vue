<template>
    <div class="sn-loading">
        <div class="sn-pulse sn-pulse1">
        </div><div class="sn-pulse sn-pulse2">
        </div><div class="sn-pulse sn-pulse3">
        </div>
    </div>
</template>
<script>
export default{
    name: "SNLoader"
}
</script>
<style lang="scss">
@import "~styles/_vars.scss";
.sn-pulse{
    background-color: $primary;
    width:15px;
    height:15px;
    margin:2px;
    border-radius: 100%;
    display: inline-block;
    animation-name:sn-pulse;
    animation-duration: 0.75s;
    animation-iteration-count: infinite;
    animation-fill-mode: both;
}
.sn-loading{
    padding:2px;
    height:19px;
    width:57px;
}

@keyframes sn-pulse{
    0%,
    80%{
        transform: scale(1);
        opacity: 1;
    }
    45%{
        transform:scale(0.1);
        opacity: 0.7;
    }
}

.sn-pulse1{
    animation-delay: 0.12s;
}
.sn-pulse2{
    animation-delay: 0.24s;
}
.sn-pulse3{
    animation-delay: 0.36s;
}
</style>