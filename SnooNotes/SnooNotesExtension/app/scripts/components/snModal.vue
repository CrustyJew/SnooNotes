<template>
  <transition name="snmodal">
    <div class="snmodal-mask" v-show="show" @click="close">
      <div id="sn-modal" class="snmodal-container" @click.stop>
        <div class="sn-header"><a class="sn-close-modal sn-close" @click="close">X</a></div>
        <slot></slot>
      </div>
    </div>
  </transition>
</template>
<script>
export default {
  props: ['show', 'onClose'],
  methods: {
    close() {
      this.onClose();
    }
  }
}
</script>
<style lang="scss">
@import "~styles/_vars.scss";
.sn-close-modal {
  display: inline-block;
  cursor: pointer;
  color: white;
  background-color: $accent;
  font-weight: bold;
  padding: 2px 10px;
  margin-top: -1px;
  &:hover {
    background-image: linear-gradient(to bottom, darken($accent, 10%), darken($accent, 25%));
  }
}

.snmodal-mask {
  position: fixed;
  z-index: 2147483646;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, .7);
  display: table;
  transition: opacity .3s ease;
}

.snmodal-wrapper {
  display: table-cell;
  vertical-align: middle;
}

.snmodal-container {
  margin: 0px auto;
  background-color: #fff;
  border-radius: 10px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, .33);
  transition: all .3s ease;
  position: fixed;
  top: 10%;
  left: 10%;
  right: 10%;
  bottom: 10%;
  z-index: 2147483647 !important;
  overflow-x: hidden;
  overflow-y: auto;
  max-height: 1200px;
}

.snmodal-header h3 {
  margin-top: 0;
  color: #42b983;
}

.snmodal-body {
  margin: 20px 0;
}

.snmodal-default-button {
  float: right;
}

.snmodal-enter {
  opacity: 0;
}

.snmodal-leave-active {
  opacity: 0;
}

.snmodal-enter .snmodal-container,
.snmodal-leave-active .snmodal-container {
  -webkit-transform: scale(1.1);
  transform: scale(1.1);
}

body.res .side .titlebox div[data-res-css]:first,
#tb-bottombar {
  z-index: auto !important;
}
</style>