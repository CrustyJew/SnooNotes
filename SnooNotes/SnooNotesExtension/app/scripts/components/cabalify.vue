<template>
    <span class="sn-cabal-container" v-if="isCabal" @click.stop>
        <span class="sn-cabalify-icon" @click="show"> </span>
        <div class="sn-cabalify" v-if="display">
            <ul class="SNNoteType">
                <li class="sn-cabal-type" v-for="(nt, index) in cabalNoteTypes" :style="noteTypeStyle(index)" @click="cabalify(nt.NoteTypeID)">
                    <i class="material-icons">{{nt.IconString}}</i>{{nt.DisplayName}}</li>
            </ul>
        </div>
    </span>
</template>
<script>
import Toasted from 'vue-toasted';
import axios from 'axios';
export default {
  props:['noteID'],
  data(){
      return {
            snInfo: this.$select('snoonotes_info as snInfo'),
            isCabal: this.$select('user.isCabal as isCabal'),
            displayStyle: {
                display: 'none',
                position: 'absolute',
                top: '0px',
                right: '0px',
            },
            display:false
      }
  },
  computed:{
        cabalNoteTypes: function(){
            if(!this.isCabal) return {};
            return this.snInfo.modded_subs.filter(sub => sub.SubName == "SpamCabal")[0].Settings.NoteTypes;
        },
  },
  methods:{
    close: function () {
        this.display = false;
        document.removeEventListener('click', this.clickWatch, true);
    },
    show: function (e) {
        //this.displayStyle.top = e.target.offsetTop + 15 + 'px';
        //this.displayStyle.left = e.target.offsetLeft + 20 + 'px';
        this.displayStyle.top = e.pageY - 10 + 'px';
        this.displayStyle.left = e.pageX + 10 + 'px'

        this.displayStyle.display = 'block';
        this.display = true;
        document.addEventListener('click', this.clickWatch, true);
    },
    clickWatch: function (e) {
        if (!this.$el.contains(e.target)) this.close();
    },
    cabalify: function(typeid){
        axios.post('Note/Cabal?id='+this.noteID + '&typeid='+typeid).
            then(()=>{this.$toasted.success("Successfully informed the cabal!");},
                ()=>{this.$toasted.error("Batphone failed to inform the cabal..")});
        this.display = false;
    },
    noteTypeStyle: function(index){
        let nt = this.cabalNoteTypes[index];
        let style = {
                color: '#' + nt.ColorCode
            }
        if (nt.Bold) style.fontWeight = "bold";
        if (nt.Italic) style.fontStyle = "italic";
        return style;
    }
  }
}
</script>
<style lang="scss">
@import "~styles/_vars.scss";

.sn-cabalify-icon {
    content: " ";
    display: inline-block;
    cursor: pointer;
    height: 16px;
    width: 16px;
    background-size: contain;
    background-image: url(data:image/png;base64,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);
}

.sn-cabalify {
    z-index: 99000;
    background-color: white;
    border: $dark-gray 1px solid;
    width: 150px;
    border-radius: 5px;
    position: absolute;

    li {
        cursor: pointer;
        padding: 3px;
    }

    li:hover {
        background-color: $light-gray;
    }
    ul {
        margin-top: 0px;
    }

    .material-icons {
        font-size: 18px;
    }
}
</style>

